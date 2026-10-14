import {
  Component,
  DestroyRef,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { TaskItem } from '../../core/models/task.model';
import { AuthService } from '../../core/services/auth.service';
import { TaskService } from '../../core/services/task.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './tasks.html'
})
export class TasksPage implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly taskService = inject(TaskService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly tasks = signal<TaskItem[]>([]);
  readonly loading = signal(false);
  readonly busyTaskId = signal<number | null>(null);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    if (this.loading() || this.busyTaskId() !== null) {
      return;
    }

    this.errorMessage.set('');
    this.loading.set(true);

    this.taskService
      .getAll()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: tasks => {
          this.tasks.set(tasks);
        },
        error: (error: HttpErrorResponse) => {
          this.showError(error);
        }
      });
  }

  toggleStatus(task: TaskItem): void {
    if (this.loading() || this.busyTaskId() !== null) {
      return;
    }

    this.errorMessage.set('');
    this.busyTaskId.set(task.id);

    this.taskService
      .toggleStatus(task.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.busyTaskId.set(null))
      )
      .subscribe({
        next: updatedTask => {
          this.tasks.update(tasks =>
            tasks.map(item =>
              item.id === updatedTask.id
                ? updatedTask
                : item
            )
          );
        },
        error: (error: HttpErrorResponse) => {
          this.showError(error);
        }
      });
  }

  deleteTask(task: TaskItem): void {
    if (this.loading() || this.busyTaskId() !== null) {
      return;
    }

    const confirmed = window.confirm(
      `Delete "${task.title}"?`
    );

    if (!confirmed) {
      return;
    }

    this.errorMessage.set('');
    this.busyTaskId.set(task.id);

    this.taskService
      .delete(task.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.busyTaskId.set(null))
      )
      .subscribe({
        next: () => {
          this.tasks.update(tasks =>
            tasks.filter(item => item.id !== task.id)
          );
        },
        error: (error: HttpErrorResponse) => {
          this.showError(error);
        }
      });
  }

  logout(): void {
    this.authService.logout();

    void this.router.navigateByUrl('/login');
  }

  private showError(error: HttpErrorResponse): void {
    if (error.status === 401) {
      // The interceptor handles logout and navigation.
      return;
    }

    if (error.status === 0) {
      this.errorMessage.set(
        'Unable to connect. Please check your connection and try again.'
      );
    } else if (error.status === 404) {
      this.errorMessage.set(
        'This task no longer exists. Refresh the list.'
      );
    } else {
      this.errorMessage.set(
        'Unable to complete the request. Please try again.'
      );
    }
  }
}