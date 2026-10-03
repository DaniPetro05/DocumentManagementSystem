import {} from 'jasmine';
import { TestBed } from '@angular/core/testing';

import { DocumentServiceService } from './document-service.service';

describe('DocumentServiceService', () => {
  let service: DocumentServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocumentServiceService);
  });

  //Dummy test
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  //WIP
  it('should subscribe to content', () => {
    expect(service.isResolved == true);
  })
});
