import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class DocumentServiceService {
  constructor(private http: HttpClient) {}
  url : string = environment.ApiUrl + '/documentmanagement';
  isResolved : boolean = false;
  subscribeContent() {
    console.log("Is executed!");
    this.http.get(this.url).subscribe({
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
