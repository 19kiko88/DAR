import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { MultiSelectModule } from 'primeng/multiselect';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TabViewModule } from 'primeng/tabview';

@NgModule({
  declarations: [
  ],
  imports: [
    FormsModule,
    CommonModule,
    TableModule,
    MultiSelectModule,
    PanelModule,
    ButtonModule,
    TabViewModule
  ],
  exports:[
    TableModule,
    MultiSelectModule,
    PanelModule,
    ButtonModule,
    TabViewModule
  ]
})
export class PrimeNgModule { }
