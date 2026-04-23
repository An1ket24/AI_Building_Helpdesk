import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Ticket, TicketComment } from './models';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/tickets';

  getTickets(): Promise<Ticket[]> {
    return firstValueFrom(this.http.get<Ticket[]>(this.apiUrl));
  }

  createTicket(payload: { issue: string; category: string; location?: string | null; priority: string }): Promise<Ticket> {
    return firstValueFrom(this.http.post<Ticket>(this.apiUrl, payload));
  }

  updateTicket(id: number, payload: { status: string; priority: string; assignedTo?: string | null }): Promise<Ticket> {
    return firstValueFrom(this.http.put<Ticket>(`${this.apiUrl}/${id}`, payload));
  }

  getComments(ticketId: number): Promise<TicketComment[]> {
    return firstValueFrom(this.http.get<TicketComment[]>(`${this.apiUrl}/${ticketId}/comments`));
  }

  addComment(ticketId: number, payload: { body: string }): Promise<TicketComment> {
    return firstValueFrom(this.http.post<TicketComment>(`${this.apiUrl}/${ticketId}/comments`, payload));
  }
}
