import { ToExp3File } from '../models/dto/request/to-exp-3file';
import { ReqDsnCompare } from '../models/dto/request/req-dsn-compare';
import { DesignDiff, ResDsnComePare } from '../models/dto/response/res-dsn-compare';
import { IResultDto } from '../models/dto/response/result-dto';
import { BaseService } from './base.service';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ResToExp3File } from '../models/dto/response/res-to-exp3-file';
import { ResGetSchematicData } from '../models/dto/response/res-get-schematic-data';


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

  ToExp3File(data: ToExp3File): Observable<IResultDto<ResToExp3File>>
  {
    const url = `${environment.apiBaseUrl}/DesignAuditRise/ToExp3File`;
    const options = this.generatePostOptions();

    return this.httpClient.post<IResultDto<ResToExp3File>>(url, data, options);
  }

  GetSchematicData(sourcefileId: string, destfileId: string, subPath: string): Observable<IResultDto<ResGetSchematicData>>
  {
    const url = `${environment.apiBaseUrl}/DesignAuditRise/GetSchematic`;
    const options = this.generatePostOptions();

    let params:string = '';
    if (sourcefileId) 
    { 
      params += `&sourcefileId=${sourcefileId}`;
    }
    if (destfileId) 
    { 
      params += `&destfileId=${destfileId}`;
    }
    if (subPath) 
    { 
      params += `&subPath=${subPath}`;
    }
    if(params)
    {
      params.substring(1, params.length);
    }
    let reUrl = `${url}?${params}`;

    return this.httpClient.get<IResultDto<ResGetSchematicData>>(reUrl, options);
  }


  DsnCompare(data: ReqDsnCompare): Observable<ResDsnComePare>
  {
    const url = `${environment.apiBaseUrl}/DesignAuditRise/DsnCompare`;
    const options = this.generatePostOptions();

    return this.httpClient.post<IResultDto<ResDsnComePare>>(url,data, options).pipe(
      map( res => this.processResult(res))
    );
  }

  ExportCompareResult(data: DesignDiff): Observable<any>
  {
    const url = `${environment.apiBaseUrl}/DesignAuditRise/ExportCompareResult`;
    const options:any = this.generatePostOptions();
    options.responseType = 'arraybuffer';

    return this.httpClient.post<any>(url, data, options)
          // .pipe(
          //   map(data => {
          //   this.downloadFile(
          //     `${filename}.xlsx`,
          //     data,
          //     'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
          //   )})
          // )
  }

  public downloadFile(name: string, data: any, type: string)
  {     
    const blob = new Blob([data], { type: type });
    const url = window.URL.createObjectURL(blob);
    var link = document.createElement('a');
    link.href = url;
    link.download = name;
    link.click();
    link.remove();
  }
}
