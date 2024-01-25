import { MainComponent } from './pages/home/components/main/main.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {path: '', component: MainComponent },
  {path: 'main', component: MainComponent },  
  {path: 'main/:sourceId/:destId', component: MainComponent },  
  {path: '**', redirectTo: 'main' }//沒有比對到路由
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
