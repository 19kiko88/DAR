import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {

  private emitLoader = new Subject<LoadingInfo>();
  loader$ = this.emitLoader.asObservable();

  constructor() { }

  setLoading(isLoading: boolean, loadingMessage?: string)
  {
    let data: LoadingInfo = { isLoading: isLoading, loadingMessage: loadingMessage ?? 'Loading'}
    this.emitLoader.next(data);//發送訂閱內容。(於LoadingComponent的OnInit訂閱)
  }
}

export interface LoadingInfo
{
  isLoading: boolean;
  loadingMessage?: string;
}
