import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/auth.service';
import { KnowledgeBaseService } from '../core/knowledge-base.service';
import { KnowledgeBaseArticle, Ticket, TicketComment, UserSummary } from '../core/models';
import { TicketService } from '../core/ticket.service';
import { ToastService } from '../core/toast.service';
import { UserService } from '../core/user.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  private readonly ticketService = inject(TicketService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly userService = inject(UserService);
  private readonly knowledgeBaseService = inject(KnowledgeBaseService);

  protected readonly tickets = signal<Ticket[]>([]);
  protected readonly technicians = signal<UserSummary[]>([]);
  protected readonly searchTerm = signal('');
  protected readonly statusFilter = signal('All');
  protected readonly priorityFilter = signal('All');
  protected readonly categoryFilter = signal('All');
  protected readonly selectedTicket = signal<Ticket | null>(null);
  protected readonly ticketComments = signal<TicketComment[]>([]);
  protected readonly commentDraft = signal('');
  protected readonly knowledgeArticles = signal<KnowledgeBaseArticle[]>([]);
  protected readonly newArticle = signal<Omit<KnowledgeBaseArticle, 'id'>>({ title: '', category: '', guidance: '', isActive: true });

  protected readonly openCount = computed(() => this.tickets().filter((ticket) => ticket.status === 'Open').length);
  protected readonly inProgressCount = computed(() => this.tickets().filter((ticket) => ticket.status === 'In Progress').length);
  protected readonly resolvedCount = computed(() => this.tickets().filter((ticket) => ticket.status === 'Resolved').length);
  protected readonly categories = computed(() => [...new Set(this.tickets().map((ticket) => ticket.category))].sort());
  protected readonly filteredTickets = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();

    return this.tickets().filter((ticket) => {
      const matchesSearch = !query
        || ticket.issue.toLowerCase().includes(query)
        || ticket.category.toLowerCase().includes(query)
        || (ticket.location || '').toLowerCase().includes(query)
        || (ticket.createdByName || '').toLowerCase().includes(query)
        || (ticket.assignedTo || '').toLowerCase().includes(query);

      const matchesStatus = this.statusFilter() === 'All' || ticket.status === this.statusFilter();
      const matchesPriority = this.priorityFilter() === 'All' || ticket.priority === this.priorityFilter();
      const matchesCategory = this.categoryFilter() === 'All' || ticket.category === this.categoryFilter();

      return matchesSearch && matchesStatus && matchesPriority && matchesCategory;
    });
  });

  async ngOnInit(): Promise<void> {
    await Promise.all([this.loadTickets(), this.loadTechnicians(), this.loadKnowledgeArticles()]);
  }

  protected async saveTicket(ticket: Ticket): Promise<void> {
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
      this.toastService.show(`Ticket #${ticket.id} updated.`, 'success');
    } catch (error) {
      console.error(error);
      this.toastService.show('Update failed.', 'error');
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

  protected async createArticle(): Promise<void> {
    const draft = this.newArticle();
    if (!draft.title.trim() || !draft.category.trim() || !draft.guidance.trim()) {
      this.toastService.show('Please complete the new knowledge article.', 'error');
      return;
    }

    try {
      const created = await this.knowledgeBaseService.create(draft);
      this.knowledgeArticles.update((items) => [...items, created]);
      this.newArticle.set({ title: '', category: '', guidance: '', isActive: true });
      this.toastService.show('Knowledge article created.', 'success');
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not create article.', 'error');
    }
  }

  protected updateNewArticle<K extends keyof Omit<KnowledgeBaseArticle, 'id'>>(field: K, value: Omit<KnowledgeBaseArticle, 'id'>[K]): void {
    this.newArticle.update((current) => ({ ...current, [field]: value }));
  }

  protected async saveArticle(article: KnowledgeBaseArticle): Promise<void> {
    try {
      const updated = await this.knowledgeBaseService.update(article);
      this.knowledgeArticles.update((items) => items.map((item) => item.id === updated.id ? updated : item));
      this.toastService.show('Knowledge article updated.', 'success');
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not update article.', 'error');
    }
  }

  protected async deleteArticle(article: KnowledgeBaseArticle): Promise<void> {
    try {
      await this.knowledgeBaseService.delete(article.id);
      this.knowledgeArticles.update((items) => items.filter((item) => item.id !== article.id));
      this.toastService.show('Knowledge article removed.', 'success');
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not remove article.', 'error');
    }
  }

  protected logout(): void {
    this.authService.logout();
  }

  private async loadTickets(): Promise<void> {
    try {
      this.tickets.set(await this.ticketService.getTickets());
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not load tickets.', 'error');
    }
  }

  private async loadTechnicians(): Promise<void> {
    try {
      this.technicians.set(await this.userService.getTechnicians());
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not load technicians.', 'error');
    }
  }

  private async loadKnowledgeArticles(): Promise<void> {
    try {
      this.knowledgeArticles.set(await this.knowledgeBaseService.getAll());
    } catch (error) {
      console.error(error);
      this.toastService.show('Could not load knowledge base.', 'error');
    }
  }
}
