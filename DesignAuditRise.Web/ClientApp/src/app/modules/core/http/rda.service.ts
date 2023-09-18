import { ToExp3File } from './../../shared/models/dto/request/to-exp-3file';
import { ReqDsnCompare } from '../../shared/models/dto/request/req-dsn-compare';
import { ResDsnComePare } from '../../shared/models/dto/response/res-dsn-compare';
import { IResultDto } from '../../shared/models/dto/response/result-dto';
import { BaseService } from './base.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { TodoItemNode } from '../../shared/ng-material/checkbox-tree/checkbox-tree.component';

@Injectable({
  providedIn: 'root'
})
export class RdaService extends BaseService {

  constructor(
    private httpClient: HttpClient
  ) 
  { 
    super();
  }

  ToExp3File(data: ToExp3File): Observable<IResultDto<TodoItemNode[]>>
  {
    const url = `${environment.apiBaseUrl}/DesignAuditRise/ToExp3File`;
    const options = this.generatePostOptions();

    return this.httpClient.post<IResultDto<TodoItemNode[]>>(url,data, options);
  }


  DsnCompare(data: ReqDsnCompare): Observable<ResDsnComePare>
  {
    const url = `${environment.apiBaseUrl}/DesignAuditRise/DsnCompare`;
    const options = this.generatePostOptions();

    return this.httpClient.post<IResultDto<ResDsnComePare>>(url,data, options).pipe(
      map( res => this.processResult(res))
    );
  }
}
