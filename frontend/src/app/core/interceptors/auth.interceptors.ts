import { inject } from '@angular/core';
import {
  HttpErrorResponse,
  HttpInterceptorFn
} from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (
  request,
  next
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isApiRequest = request.url.startsWith('/api/');
  const isLoginRequest = request.url === '/api/auth/login';

  if (!isApiRequest || isLoginRequest) {
    return next(request);
  }

  const token = authService.getToken();

  const authenticatedRequest = token
    ? request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : request;

  return next(authenticatedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();

        void router.navigateByUrl('/login');
      }

      return throwError(() => error);
    })
  );
};