import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { UserSummary } from './models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/users';

  getTechnicians(): Promise<UserSummary[]> {
    return firstValueFrom(this.http.get<UserSummary[]>(`${this.apiUrl}/technicians`));
  }
}
