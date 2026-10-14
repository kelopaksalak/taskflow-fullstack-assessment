import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { LoginPage } from './features/login/login';
import { TasksPage } from './features/tasks/tasks';
import { TaskFormPage } from './features/tasks/task-form';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'tasks',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginPage
  },
  {
    path: 'tasks/new',
    component: TaskFormPage,
    canActivate: [authGuard]
  },
  {
    path: 'tasks/:id/edit',
    component: TaskFormPage,
    canActivate: [authGuard]
  },
  {
    path: 'tasks',
    component: TasksPage,
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: 'tasks'
  }
];