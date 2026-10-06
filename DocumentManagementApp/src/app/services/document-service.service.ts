import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Document } from '../document.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class DocumentServiceService {
  constructor(public http: HttpClient) {}
  url : string = environment.ApiUrl + '/documentmanagement';
  list: Document[] = [];
  isResolved : boolean = false;
  /*subscribeContent() {
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
  }*/

  subscribeContent(): void {
    console.log('Loading documents...');

    this.http.get<Document[]>(this.url).subscribe({
      next: resolve => {
        console.log('Documents received:', resolve);
        this.list = resolve;
        this.isResolved = true;
      },
      error: err => {
        console.error('Error loading documents:', err);
        this.isResolved = false;
      }
    });
  }

  createDocument(document: Document): Observable<Document> {
    console.log('Creating document:', document);

    return this.http.post<Document>(this.url, document);
  }
}
