import {inject, Injectable} from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, tap } from "rxjs";

import { LoginRequest, LoginResponse } from "../models/auth.model";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly http = inject(HttpClient);

    private readonly tokenKey =  'taskflow_token';
    private readonly expiryKey = 'taskflow_token_expiry';

    login(request:LoginRequest): Observable<LoginResponse>  {
        return this.http
            .post<LoginResponse>('/api/auth/login', request)
            .pipe(
                tap(response => {
                    localStorage.setItem(this.tokenKey, response.token);
                    localStorage.setItem(this.expiryKey, response.expiresAtUtc);
                })
            );
    }

    getToken(): string | null {
        if (!this.isAuthenticated()){
            return null;
        }

        return localStorage.getItem(this.tokenKey);
    }

    isAuthenticated(): boolean {
        const token = localStorage.getItem(this.tokenKey);
        const expiry = localStorage.getItem(this.expiryKey);

        if(!token || !expiry){
            this.logout();
            return false;
        }

        const expiryTime = Date.parse(expiry);

        if (!Number.isFinite(expiryTime) || Date.now() >= expiryTime) {
            this.logout();
            return false;
        }

        return true;
    }

    logout(): void {
        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.expiryKey);
    }
}