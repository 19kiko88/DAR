import { CompareErrorResult } from '../../core/models/compare-error-result';

// @Injectable({
//   providedIn: 'root'
// })
export class PostMessageService 
{
  private _currentRdcsId: string = '';
  get currentRdcsId()
  {
   return this._currentRdcsId;
  }

  private _schematicWindows:{isRegister: boolean, name:string, source:any}[] = [/*{isRegister:false, name:'', source:null}*/];
  get schematicWindows()
  {
    return this._schematicWindows.filter(c => c.name != '');
  }
  // set schematicWindows(value: any)
  // {
  //   this._schematicWindows = value;
  // }

  private _schematicViewerUrl: string= '';
  get schematicViewerUrl()
  {
    return this._schematicViewerUrl;
  }
  set schematicViewerUrl(value: string)
  {
    this._schematicViewerUrl = value;
  }

  private _pmEvent?: MessageEvent;
  get pmEvent()
  {
    return this._pmEvent;
  }
  set pmEvent(value: any)
  {
    this._pmEvent = value;
    this._receivePmData = this._pmEvent?.data;
  }

  private _receivePmData?: PostMessageData<any>;
  get receivePmData()
  {
    return this._receivePmData;
  }

  constructor(){}

  broadcast(selfSchematicViewerName: string)
  {
    let usefulSchematicWindows = this._schematicWindows.filter(c => c.name != '' && c.name != selfSchematicViewerName);

    for (var i = 0; i < usefulSchematicWindows.length; i++) 
    {
      if (usefulSchematicWindows[i].source.frames !== null) 
      {
        let postPmData: PostMessageData<any> = {name:CmdType.broadcast, cmd:this.receivePmData?.cmd ?? '', paras: this.receivePmData?.paras};
        usefulSchematicWindows[i].source.postMessage(postPmData, this._schematicViewerUrl);
      }        
    }
  }

  close(all:boolean = false, rdcsSn: string = '')
  {
    let usefulSchematicWindows = !all ? this._schematicWindows.filter(c => c.name != '' && c.name.indexOf(rdcsSn) <= 0) : this._schematicWindows;

    for (var i = 0; i < usefulSchematicWindows.length; i++) 
    {
      if (usefulSchematicWindows[i].source.frames !== null) 
      {
        let postPmData: PostMessageData<any> = {name:CmdType.broadcast, cmd:CmdType.close, paras: null};
        usefulSchematicWindows[i].source.postMessage(postPmData, this._schematicViewerUrl);
      }        
    }
  }

  register(window: Window, postData: CompareErrorResult)
  {
    let receivePmData = this._pmEvent?.data;

    let existed = 
    this._schematicWindows
    .filter(c =>
      c.isRegister == true && c.name == receivePmData?.name && c.source == this._pmEvent?.source
    ).length > 0? true : false;

    if (!existed)
    {
      this._schematicWindows.push({isRegister: true, name: receivePmData?.name, source: this._pmEvent?.source });
      this._currentRdcsId = receivePmData?.name.split('_')[1]; 
    }

    if (postData) 
    {
      let copyPostData = JSON.parse(JSON.stringify(postData));
      this.propertyNameWithUppercaseStart(copyPostData.partErrors);
      this.propertyNameWithUppercaseStart(copyPostData.netErrors);
      this.propertyNameWithUppercaseStart(copyPostData.comparePageMapping);

      let postPmData:PostMessageData<string> = 
      {
        name: receivePmData.name,
        cmd: receivePmData.cmd,
        paras:JSON.stringify(copyPostData)
      }

      window.postMessage(postPmData, this._schematicViewerUrl);
    }    
  }

  /**
   * property name開頭第一個字轉成大寫，以符合SchematicViewer欄位名稱
   * @param data
   */
  propertyNameWithUppercaseStart(data:any)
  {
   for(var i = 0; i<data.length;i++) 
   {
     var a = data[i];
     for (var key in a) {
         if (a.hasOwnProperty(key)) {
           a[key.charAt(0).toUpperCase() + key.substring(1)] = a[key];
           delete a[key];              
         }
     }
     data[i] = a;    
   }
  }
}

export interface PostMessageData<T>
{
  name:string;
  cmd: string;
  paras: T;
}

export class CmdType
{
    public static readonly register: string = "register";
    public static readonly moveToPageName: string = "moveToPageName";
    public static readonly draw: string = "draw";
    public static readonly searchResultClick: string = "searchResultClick";
    public static readonly zoom: string = "zoom";
    public static readonly closePage: string = "closePage";
    public static readonly moveToPage: string = "moveToPage";
    public static readonly markPosition: string = "markPosition";
    public static readonly close: string = "close";
    public static readonly broadcast: string = "broadcast";
}
