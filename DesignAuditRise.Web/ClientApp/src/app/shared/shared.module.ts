import { PrimeNgModule } from './prime-ng/prime-ng.module';
import { NgMaterialModule } from './ng-material/ng-material.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UploadComponent } from './components/upload/upload.component';
import { LoadingComponent } from './components/loading/loading.component';
import { NgBootstrapModule } from './ng-bootstrap/ng-bootstrap.module';

@NgModule({
  declarations: [
    UploadComponent,
    LoadingComponent    
  ],
  imports: [
    PrimeNgModule,
    NgMaterialModule,
    NgBootstrapModule,
    CommonModule
  ],
  exports: [        
    PrimeNgModule,
    NgMaterialModule,
    NgBootstrapModule,
    UploadComponent,
    LoadingComponent
  ]
})
export class SharedModule { }
