import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DocumentServiceService } from '../services/document-service.service';
import { Observable } from 'rxjs';
import { Document } from '../document.model';
import { HttpClient } from '@angular/common/http';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-document-details',
  standalone: true, //stated explicilty for directly putting in route configuration
  imports: [FormsModule],
  templateUrl: './document-details.component.html',
  styleUrl: './document-details.component.css'
})
export class DocumentDetailsComponent implements OnInit {
  newDocument: Document = {
    id: 0,
    title: '',
    format: ''
  };

  message: string = '';
  errorMessage: string = '';
  
  constructor(public DocumentService : DocumentServiceService) {}
  ngOnInit(): void {
    console.log('DocumentDetailsComponent initialized');
      this.DocumentService.subscribeContent();
  }

  createDocument(): void {
    this.message = '';
    this.errorMessage = '';

    if (!this.newDocument.title.trim()) {
      this.errorMessage = 'Please enter a document title.';
      return;
    }

    if (!this.newDocument.format.trim()) {
      this.errorMessage = 'Please enter a document format.';
      return;
    }

    this.DocumentService.createDocument(this.newDocument).subscribe({
      next: createdDocument => {
        console.log('Document created:', createdDocument);

        this.message = 'Document created successfully!';

        this.DocumentService.list.push(createdDocument);

        this.newDocument = {
          id: 0,
          title: '',
          format: ''
        };
      },

      error: err => {
        console.error('Error creating document:', err);
        this.errorMessage = 'Could not create the document.';
      }
    });
  }
}
