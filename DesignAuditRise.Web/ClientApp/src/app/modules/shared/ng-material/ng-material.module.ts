import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CheckboxTreeComponent } from './checkbox-tree/checkbox-tree.component';

import {CdkTreeModule} from '@angular/cdk/tree';
import {MatTreeModule} from '@angular/material/tree';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';

@NgModule({
  declarations: [
    CheckboxTreeComponent
  ],
  imports: [
    CommonModule,
    CdkTreeModule,
    MatTreeModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatButtonModule,
    MatCardModule
  ],
  exports:[    
    CdkTreeModule,
    MatTreeModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatButtonModule,
    MatCardModule,
    CheckboxTreeComponent
  ]
})

export class NgMaterialModule { }
