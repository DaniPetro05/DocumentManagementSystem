import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Document } from '../document.model';

@Injectable({
  providedIn: 'root'
})

export class DocumentServiceService {
  constructor(public http: HttpClient) {}
  url : string = environment.ApiUrl + '/documentmanagement';
  list: Document[] = [];
  isResolved : boolean = false;
  subscribeContent() {
    console.log("Is executed!");
    this.http.get(this.url).subscribe({
      next: resolve => {
        console.log(resolve);
        this.list = resolve as Document[];
        this.isResolved = true;
      },
      error: err => {
        console.log(err);
        this.isResolved = false;
      }
    })
  }
}
