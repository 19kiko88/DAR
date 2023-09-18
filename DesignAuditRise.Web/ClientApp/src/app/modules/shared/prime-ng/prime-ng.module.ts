import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { MultiSelectModule } from 'primeng/multiselect';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';

@NgModule({
  declarations: [
  ],
  imports: [
    FormsModule,
    CommonModule,
    TableModule,
    MultiSelectModule,
    PanelModule,
    ButtonModule
  ],
  exports:[
    TableModule,
    MultiSelectModule,
    PanelModule,
    ButtonModule
  ]
})
export class PrimeNgModule { }
