import { Routes } from '@angular/router';
import { DocumentDetailsComponent } from './document-details/document-details.component';

/*export const routes: Routes = [{
    path: 'document-details', component: DocumentDetailsComponent
}];*/

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'document-details',
    pathMatch: 'full'
  },
  {
    path: 'document-details',
    component: DocumentDetailsComponent
  }
];
