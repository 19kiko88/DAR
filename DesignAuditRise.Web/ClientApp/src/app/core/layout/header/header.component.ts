import { Component } from '@angular/core';
import { CommonService } from '../../http/common.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent  {

  userName:string | null = '';

  constructor
  (
    private _commonService: CommonService
  )
  {
    this.userName = localStorage.getItem('userName');
    if(!this.userName)
    {
      this._commonService.GetUserInfo().subscribe(
        {
          next: res => {
            localStorage.setItem('userName', res);
            this.userName = res    
          },
          error: err => {
            this.userName = 'Unknow User'
          }
        }
      );
    }    
  }   
}
