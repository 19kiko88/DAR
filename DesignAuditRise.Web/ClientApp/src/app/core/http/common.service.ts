import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IResultDto } from '../models/dto/response/result-dto';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class CommonService extends BaseService {

  constructor(
    private httpClient: HttpClient
  ) 
  { 
    super();
  }


  GetUserInfo(): Observable<string>
  {
    const url = `${environment.apiBaseUrl}/Common/GetUserInfo`;
    const options = this.generatePostOptions();

    return this.httpClient.get<IResultDto<string>>(url, options).pipe(
      map( res => this.processResult(res))
    );
  }
}
