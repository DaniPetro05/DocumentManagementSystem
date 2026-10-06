import { Component, OnInit } from '@angular/core';
import { DocumentServiceService } from '../services/document-service.service';
import { Observable } from 'rxjs';
import { Document } from '../document.model';
import { HttpClient } from '@angular/common/http';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-document-details',
  imports: [],
  templateUrl: './document-details.component.html',
  styleUrl: './document-details.component.css'
})
export class DocumentDetailsComponent implements OnInit {
  constructor(public DocumentService : DocumentServiceService) {}
  ngOnInit(): void {
    console.log('DocumentDetailsComponent initialized');
      this.DocumentService.subscribeContent();
  }
}
