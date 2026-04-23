import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { KnowledgeBaseArticle } from './models';

@Injectable({ providedIn: 'root' })
export class KnowledgeBaseService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/knowledgebase';

  getAll(): Promise<KnowledgeBaseArticle[]> {
    return firstValueFrom(this.http.get<KnowledgeBaseArticle[]>(this.apiUrl));
  }

  create(payload: Omit<KnowledgeBaseArticle, 'id'>): Promise<KnowledgeBaseArticle> {
    return firstValueFrom(this.http.post<KnowledgeBaseArticle>(this.apiUrl, payload));
  }

  update(article: KnowledgeBaseArticle): Promise<KnowledgeBaseArticle> {
    return firstValueFrom(this.http.put<KnowledgeBaseArticle>(`${this.apiUrl}/${article.id}`, article));
  }

  delete(id: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.apiUrl}/${id}`));
  }
}
