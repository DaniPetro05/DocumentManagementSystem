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

  createDocumentWithFile(title: string, format: string, file: File): Observable<Document> {
      console.log('Creating document with file:', title, format, file.name);

      const formData = new FormData();
      formData.append('title', title);
      formData.append('format', format);
      formData.append('file', file, file.name);

      return this.http.post<Document>(this.url, formData);
  }

  createDocument(document: Document): Observable<Document> {
    console.log('Creating document:', document);

    return this.http.post<Document>(this.url, document);
  }

  updateDocument(document: Document) {
    return this.http.put<void>(`${this.url}/${document.id}`, document);
  }

  deleteDocument(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
