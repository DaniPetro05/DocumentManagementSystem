import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DocumentServiceService } from '../services/document-service.service';
import { Document } from '../document.model';

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

  selectedFile: File | null = null;
  inferredFormat: string = '';

  editingDocumentId: number | null = null;

  message: string = '';
  errorMessage: string = '';
  
  constructor(public DocumentService : DocumentServiceService) {}

  ngOnInit(): void {
    console.log('DocumentDetailsComponent initialized');
    this.DocumentService.subscribeContent();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.inferredFormat = this.inferFormat(this.selectedFile);
    } else {
      this.selectedFile = null;
      this.inferredFormat = '';
    }
  }

  private inferFormat(file: File): string {
    //Benutzt die Dateiendung im Uppercase
    const ext = file.name.split('.').pop()?.toUpperCase() ?? '';
    return ext;
  }

  /*createDocument(): void {
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
  }*/

  createDocument(): void {
    this.message = '';
    this.errorMessage = '';

    if (!this.newDocument.title.trim()) {
      this.errorMessage = 'Please enter a document title.';
      return;
    }

    if (this.editingDocumentId !== null) {
      this.updateDocument();
      return;
    }

    /*if (!this.newDocument.format.trim()) {
      this.errorMessage = 'Please enter a document format.';
      return;
    }*/

    /*this.DocumentService.createDocument(this.newDocument).subscribe({
      next: createdDocument => {
        this.message = 'Document created successfully!';
        this.DocumentService.list.push(createdDocument);
        this.newDocument = { id: 0, title: '', format: '' };
      },
      error: err => {
        console.error('Error creating document:', err);
        this.errorMessage = 'Could not create the document.';
      }
    });*/

    if (!this.selectedFile) {
      this.errorMessage = 'Please select a file to upload.';
      return;
    }

    this.DocumentService.createDocumentWithFile(
      this.newDocument.title,
      this.inferredFormat,
      this.selectedFile
    ).subscribe({
      next: createdDocument => {
        this.message = 'Document created successfully!';
        this.DocumentService.list.push(createdDocument);
        this.resetForm();
      },
      error: err => {
        console.error('Error creating document:', err);
        this.errorMessage = 'Could not create the document.';
      }
    });
  }

  editDocument(document: Document): void {
    this.message = '';
    this.errorMessage = '';

    this.editingDocumentId = document.id;
    this.newDocument = { ...document };
  }

  updateDocument(): void {
    const id = this.editingDocumentId;

    if (id === null) {
      return;
    }

    const updatedDocument: Document = {
      ...this.newDocument,
      id
    };

    this.DocumentService.updateDocument(updatedDocument).subscribe({
      next: () => {
        const index = this.DocumentService.list.findIndex(
          doc => doc.id === id
        );

        if (index !== -1) {
          this.DocumentService.list[index] = updatedDocument;
        }

        this.message = 'Document updated successfully!';
        this.resetForm();
      },
      error: err => {
        console.error('Error updating document:', err);
        this.errorMessage = 'Could not update the document.';
      }
    });
  }

  deleteDocument(document: Document): void {
    this.message = '';
    this.errorMessage = '';

    const confirmed = confirm(
      `Are you sure you want to delete "${document.title}"?`
    );

    if (!confirmed) {
      return;
    }

    this.DocumentService.deleteDocument(document.id).subscribe({
      next: () => {
        this.DocumentService.list =
          this.DocumentService.list.filter(
            doc => doc.id !== document.id
          );

        if (this.editingDocumentId === document.id) {
          this.resetForm();
        }

        this.message = 'Document deleted successfully!';
      },
      error: err => {
        console.error('Error deleting document:', err);
        this.errorMessage = 'Could not delete the document.';
      }
    });
  }

  convertDocument(document: Document): void {
    //Conversion wird in einem anderem Sprint implementiert
    this.message =
      `Document conversion for "${document.title}" is not available yet.`;
    this.errorMessage = '';
  }

  cancelEdit(): void {
    this.resetForm();
    this.message = '';
    this.errorMessage = '';
  }

  private resetForm(): void {
    this.editingDocumentId = null;

    this.newDocument = {
      id: 0,
      title: '',
      format: ''
    };

    this.selectedFile = null;
    this.inferredFormat = '';
  }
}
