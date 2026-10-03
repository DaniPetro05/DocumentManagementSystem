import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DocumentServiceService {
  constructor(private http: HttpClient) { }
  isResolved : boolean = false;
  subscribeContent() {
    this.http.get(environment.ApiUrl).subscribe({
      next: resolve => {
        console.log(resolve);
        this.isResolved = true;
      },
      error: err => {
        console.log(err);
        this.isResolved = false;
      }
    })
  }
}
