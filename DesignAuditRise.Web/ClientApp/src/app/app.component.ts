import { Component, OnInit } from '@angular/core';
import { LoadingService } from './modules/shared/components/loading/loading.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  title = 'LayouteCAE DAR';
  isLoader: boolean = false;
  loadingMsg: string|undefined = '';
  
  constructor
  (
    private _loadingService: LoadingService
  )
  {

  }

  ngOnInit(){
        //Setting Loading
        this._loadingService.loader$.subscribe(res => {
          this.isLoader = res.isLoading;
          this.loadingMsg = res.loadingMessage;
        })
  }
}
