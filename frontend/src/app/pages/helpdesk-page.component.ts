import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { ChatService } from '../core/chat.service';
import { ChatAnalysis, MessageBubble, Ticket, TicketComment } from '../core/models';
import { TicketService } from '../core/ticket.service';
import { ToastService } from '../core/toast.service';

@Component({
  selector: 'app-helpdesk-page',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './helpdesk-page.component.html',
  styleUrl: './helpdesk-page.component.css'
})
export class HelpdeskPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly chatService = inject(ChatService);
  private readonly ticketService = inject(TicketService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly session = this.authService.session;
  protected readonly isTechnician = computed(() => this.authService.role() === 'Technician');
  protected readonly messages = signal<MessageBubble[]>([
    { sender: 'bot', text: 'Hello. Describe your building issue and I will suggest a quick fix first.' }
  ]);
  protected readonly tickets = signal<Ticket[]>([]);
  protected readonly draft = signal('');
  protected readonly isSending = signal(false);
  protected readonly pendingAnalysis = signal<ChatAnalysis | null>(null);
  protected readonly typing = signal(false);
  protected readonly selectedTicket = signal<Ticket | null>(null);
  protected readonly ticketComments = signal<TicketComment[]>([]);
  protected readonly commentDraft = signal('');
  protected readonly hasTickets = computed(() => this.tickets().length > 0);

  async ngOnInit(): Promise<void> {
    await this.loadTickets();
  }

  protected async sendMessage(): Promise<void> {
    const message = this.draft().trim();
    if (!message || this.isSending()) {
      return;
    }

    this.messages.update((items) => [...items, { sender: 'user', text: message }]);
    this.draft.set('');
    this.isSending.set(true);
    this.typing.set(true);
    this.pendingAnalysis.set(null);

    try {
      const analysis = await this.chatService.analyze(message);
      this.messages.update((items) => [...items, { sender: 'bot', text: analysis.botMessage }]);
      this.pendingAnalysis.set(!this.isTechnician() && analysis.shouldOfferTicket ? analysis : null);

      if (analysis.requiresHumanHandoff) {
        this.toastService.show('Human follow-up recommended for this issue.', 'info');
      }
    } catch (error) {
      console.error(error);
      this.messages.update((items) => [...items, { sender: 'bot', text: 'I could not process that message right now. Please try again.' }]);
      this.toastService.show('Chat request failed.', 'error');
    } finally {
      this.isSending.set(false);
      this.typing.set(false);
    }
  }

  protected async confirmTicketCreation(confirmed: boolean): Promise<void> {
    const analysis = this.pendingAnalysis();
    if (!analysis || this.isTechnician()) {
      return;
    }

    if (!confirmed) {
      this.messages.update((items) => [...items, { sender: 'bot', text: 'No problem. I will keep this as a suggestion only.' }]);
      this.pendingAnalysis.set(null);
      return;
    }

    try {
      await this.ticketService.createTicket({
        issue: analysis.issue,
        category: analysis.category,
        location: analysis.location,
        priority: analysis.priority
      });
      this.messages.update((items) => [...items, { sender: 'bot', text: 'Your ticket has been created successfully.' }]);
      this.toastService.show('Ticket created.', 'success');
      this.pendingAnalysis.set(null);
      await this.loadTickets();
    } catch (error) {
      console.error(error);
      this.toastService.show('Ticket creation failed.', 'error');
    }
  }

  protected async openTicket(ticket: Ticket): Promise<void> {
    this.selectedTicket.set(ticket);
    this.commentDraft.set('');

    try {
      this.ticketComments.set(await this.ticketService.getComments(ticket.id));
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not load comments.', 'error');
    }
  }

  protected async addComment(): Promise<void> {
    const ticket = this.selectedTicket();
    const body = this.commentDraft().trim();
    if (!ticket || !body) {
      return;
    }

    try {
      const comment = await this.ticketService.addComment(ticket.id, { body });
      this.ticketComments.update((items) => [...items, comment]);
      this.commentDraft.set('');
      this.toastService.show('Comment added.', 'success');
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not add comment.', 'error');
    }
  }

  protected async saveTechnicianUpdate(ticket: Ticket): Promise<void> {
    try {
      const updated = await this.ticketService.updateTicket(ticket.id, {
        status: ticket.status,
        priority: ticket.priority,
        assignedTo: ticket.assignedTo ?? ''
      });

      this.tickets.update((items) => items.map((item) => item.id === updated.id ? updated : item));
      if (this.selectedTicket()?.id === updated.id) {
        this.selectedTicket.set(updated);
      }
      this.toastService.show('Ticket progress updated.', 'success');
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not update ticket.', 'error');
    }
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected goToAdmin(): void {
    this.router.navigateByUrl('/admin');
  }

  private async loadTickets(): Promise<void> {
    try {
      this.tickets.set(await this.ticketService.getTickets());
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not load tickets.', 'error');
    }
  }
}
