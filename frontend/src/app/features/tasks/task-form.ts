import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { SaveTaskRequest, TaskPriority } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';

function localToday(): string {
  const today = new Date();

  const year = today.getFullYear();
  const month = String(today.getMonth() + 1).padStart(2, '0');
  const day = String(today.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

const notBeforeToday: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as string;

  if (!value) {
    return null;
  }

  return value < localToday() ? { pastDate: true } : null;
};

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './task-form.html',
})
export class TaskFormPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private taskId: number | null = null;

  readonly isEdit = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly loadFailed = signal(false);
  readonly errorMessage = signal('');

  get today(): string {
    return localToday();
  }

  readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)]],
    description: ['', Validators.maxLength(200)],
    priority: this.formBuilder.nonNullable.control<TaskPriority>('Medium', Validators.required),
    dueDate: ['', notBeforeToday],
  });

  ngOnInit(): void {
    const idParameter = this.route.snapshot.paramMap.get('id');

    if (idParameter === null) {
      return;
    }

    this.isEdit.set(true);

    const id = Number(idParameter);

    if (!Number.isSafeInteger(id) || id <= 0) {
      this.loadFailed.set(true);
      this.errorMessage.set('Invalid task ID.');
      return;
    }

    this.taskId = id;
    this.loading.set(true);

    this.taskService
      .getById(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (task) => {
          this.form.setValue({
            title: task.title,
            description: task.description ?? '',
            priority: task.priority,
            dueDate: task.dueDate ?? '',
          });
        },
        error: (error: HttpErrorResponse) => {
          this.loadFailed.set(true);
          this.showError(error);
        },
      });
  }

  submit(): void {
    if (this.loading() || this.saving() || this.loadFailed()) {
      return;
    }

    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();

    const request: SaveTaskRequest = {
      title: values.title.trim(),
      description: values.description.trim() || null,
      priority: values.priority,
      dueDate: values.dueDate || null,
    };

    const operation =
      this.taskId === null
        ? this.taskService.create(request)
        : this.taskService.update(this.taskId, request);

    this.saving.set(true);

    operation
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/tasks');
        },
        error: (error: HttpErrorResponse) => {
          this.showError(error);
        },
      });
  }

  private showError(error: HttpErrorResponse): void {
    if (error.status === 401) {
      // The authentication interceptor handles this.
      return;
    }

    if (error.status === 404) {
      this.errorMessage.set('This task no longer exists. Return to the task list.');
    } else if (error.status === 400) {
      this.errorMessage.set('Please check the fields and enter a valid due date.');
    } else if (error.status === 0) {
      this.errorMessage.set('Unable to connect. Please check your connection.');
    } else {
      this.errorMessage.set('Unable to complete the request. Please try again.');
    }
  }
}
