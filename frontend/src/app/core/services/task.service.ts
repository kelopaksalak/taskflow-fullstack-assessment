import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  SaveTaskRequest,
  TaskItem
} from '../models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/tasks';

  getAll(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(this.apiUrl);
  }

  getById(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(
      `${this.apiUrl}/${id}`
    );
  }

  create(request: SaveTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(
      this.apiUrl,
      request
    );
  }

  update(
    id: number,
    request: SaveTaskRequest
  ): Observable<TaskItem> {
    return this.http.put<TaskItem>(
      `${this.apiUrl}/${id}`,
      request
    );
  }

  toggleStatus(id: number): Observable<TaskItem> {
    return this.http.patch<TaskItem>(
      `${this.apiUrl}/${id}/status`,
      {}
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }
}