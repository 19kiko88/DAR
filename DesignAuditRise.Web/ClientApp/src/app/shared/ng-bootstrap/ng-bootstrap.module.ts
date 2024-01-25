import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';



@NgModule({
  declarations: [],
  imports: [
    NgbNavModule,
    CommonModule
  ],
  exports:[
    NgbNavModule
  ]
})
export class NgBootstrapModule { }
