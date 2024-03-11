import { environment } from 'src/environments/environment';
import { CmdType, PostMessageService } from '../../../../core/service/post-message.service';
import { CompareErrorResult } from '../../../../core/models/compare-error-result';
import { PartCompareData, NetCompareData, MultipleSelectObj, DesignDiff, ResDsnComePare } from '../../../../core/models/dto/response/res-dsn-compare';
import { ToExp3File, EnumFileType } from '../../../../core/models/dto/request/to-exp-3file';
import { ReqDsnCompare } from '../../../../core/models/dto/request/req-dsn-compare';
import { enumUploadStatus, UploadComponent } from '../../../../shared/components/upload/upload.component';
import { SweetAlertService } from '../../../../core/service/sweet-alert.service';
import { RdaService } from '../../../../core/http/rda.service';
import { CheckboxTreeComponent, TodoItemNode } from '../../../../shared/ng-material/checkbox-tree/checkbox-tree.component';
import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators} from '@angular/forms';
import { BehaviorSubject, concatMap, of } from 'rxjs';
import { LoadingService } from 'src/app/shared/components/loading/loading.service';
import * as moment from 'moment';
import { TableDataSource } from '../../../../core/models/table-data-source';
import { Table } from 'primeng/table';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { SchematicViewerWindow } from 'src/app/core/models/schematic-viewer-window';
import { SweetAlertOptions } from 'sweetalert2';


@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
  providers:[PostMessageService]
})

export class MainComponent implements OnInit 
{
  @ViewChild('sourceSchematic', { static: true }) sourceSchematic!: CheckboxTreeComponent;
  @ViewChild('destinationSchematic', { static: true }) destinationSchematic!: CheckboxTreeComponent;
  @ViewChild('uploadDsn1', { static: true }) uploadDsn1!: UploadComponent;
  @ViewChild('uploadDsn2', { static: true }) uploadDsn2!: UploadComponent;
  @ViewChild('hiddenBtn') b1 !: HTMLElement;

  form!:FormGroup;
  cnCompareMode: string = "";
  compareModeIcon: string = "";
  compareMode: string = "";
  compareMode$ = new BehaviorSubject("0");
  sourceFileUploadMsg:string="";
  destinationFileUploadMsg:string="";
  active:number = 1;
  dataSource: TableDataSource = { 
    PartData: { PartDiffs:[], Design1PageGroup:[], Design2PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] },
    NetData: { NetDiffs:[], Design1PageGroup:[], Design2PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] }
  };
  filterCondition: TableDataSource = { 
    PartData: { PartDiffs:[], Design1PageGroup:[], Design2PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] },
    NetData: { NetDiffs:[], Design1PageGroup:[], Design2PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] }
  };
  selectedPartDatas!: PartCompareData;
  selectedNetDatas!: NetCompareData;
  oldSourceFileName:string='';
  oldDistinationFileName:string='';

  w1?: Window;
  w2?: Window;
  compareErrorResult: CompareErrorResult = {
    designSN1:'', 
    designSN2:'', 
    design1_2:[], 
    design2_1:[], 
    partErrors:[], 
    netErrors:[], 
    comparePageMapping:[]
  }

  sourceId: string = '';
  destId: string = '';

  constructor
  (
    private _rdaService: RdaService,
    private _salService: SweetAlertService,
    private _loadingService: LoadingService,
    private _formBuilder: FormBuilder,
    private _postMessageService: PostMessageService,
    private datepipe: DatePipe,
    private route:ActivatedRoute
  )
  {
    this._postMessageService.schematicViewerUrl = environment.schematicViewerUrl;
  } 

  ngOnInit(): void
  {  
    this.route.queryParams.pipe(
      concatMap(param => {
        this.sourceId = param['sourceId']??'';
        this.destId = param['destId']??'';

        if(this.sourceId || this.destId)
        {
          //由API取得Schematic資料
          this._loadingService.setLoading(true, "線路圖自動載入中...");
          return this._rdaService.GetSchematicData(this.sourceId, this.destId, 'OrCad');
        } 
        else 
        {
          return of(undefined);
        }        
      })
    )
    .subscribe(
      {
        next: res =>
        {
          if (res != undefined)
          {
            let sourceSchematicData = res.content.Source;
            let destSchematicData = res.content.Destination;

            if (sourceSchematicData)
            {
              this.sourceSchematic.dataSource.data = sourceSchematicData;
              this.sourceSchematic.expandAll();  
            }
          
            if (destSchematicData)
            {
              this.destinationSchematic.dataSource.data = destSchematicData;
              this.destinationSchematic.expandAll();  
            }
          }
          this._loadingService.setLoading(false);
        },
        error: err => 
        {
          this._loadingService.setLoading(false);
          this._salService.showSwal("", `線路圖自動載入發生異常，請聯繫LayoutCAE Team. ${err}`, "error");
        }
      }
    )

    //主視窗關閉事件
    window.addEventListener("beforeunload", () => {
      this._postMessageService.close(true);//關閉所有Schematic Viewer視窗.
    }, false);


    let initialTreeData:TodoItemNode[] = [];
    this.sourceSchematic.dataSource.data = initialTreeData;
    this.destinationSchematic.dataSource.data = initialTreeData;

    if(false)
    {//分頁假資料顯示 
        this.sourceSchematic.dataSource.data = [{
          item: "SCHEMATIC1", 
          children: [{item:"079_GPU_DP/EDP/HDMI/LVDS/CRT", children:[]}, {item:"083_PW_+1.8VSUS_SYV736URHC", children:[]}, {item:"089_PW_CHARGER_BB", children:[]}]
        }];
        this.destinationSchematic.dataSource.data = [{
          item: "SCHEMATIC1", 
          children: [{item:"079_GPU_DP/EDP/HDMI/LVDS/CRT", children:[]}, {item:"083_PW_+1.8VSUS_SYV736URHC", children:[]}, {item:"089_PW_CHARGER_BB", children:[]}]
        }];

        //p-multiSelect內容(ex：Page)一定要使用包含name屬性的物件
        this.dataSource.PartData.PartDiffs = [
          {objID: '1000', schematic: 'SCHEMATIC11', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, reference: 'D4C0104', part_Reference: 'D4C0104', error_CategoryForMultipleSelect: {name:'Property Changed1'}, error_DescriptionForMultipleSelect: {name:'Optional'}, design1: 'mbs_c0603', design2:'mbs_c0201_p0402', comment: '100NF/16V'},
          {objID: '1001', schematic: 'SCHEMATIC22', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, reference: 'D4C0101', part_Reference: 'D4C0101', error_CategoryForMultipleSelect: {name:'Property Changed2'}, error_DescriptionForMultipleSelect: {name:'Part Number'}, design1: '/X/DDR4', design2:'/DDR4', comment: ''},
          {objID: '1002', schematic: 'SCHEMATIC33', design1PageForMultipleSelect: {name:'PAGE3'}, design2PageForMultipleSelect: {name:'PAGE4'}, reference: 'D4C0101', part_Reference: 'D4C0101', error_CategoryForMultipleSelect: {name:'Property Changed3'}, error_DescriptionForMultipleSelect: {name:'PCB Footprint'}, design1: '11V233475150', design2:'11202V0027D000', comment: ''},
          {objID: '1003', schematic: 'SCHEMATIC44', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, reference: 'D4C0101', part_Reference: 'D4C0101', error_CategoryForMultipleSelect: {name:'Property Changed4'}, error_DescriptionForMultipleSelect: {name:'Value'}, design1: '4.7UF/6.3V', design2:'100NF/16V', comment: ''}
        ];  
        
        this.dataSource.PartData.Design1PageGroup =  this.getDistinct(this.dataSource.PartData.PartDiffs.map(c => c.design1PageForMultipleSelect));
        this.dataSource.PartData.Design2PageGroup =  this.getDistinct(this.dataSource.PartData.PartDiffs.map(c => c.design2PageForMultipleSelect));
        this.dataSource.PartData.ErrorCategoryGroup = [{name:'Property Changed'}, {name:'Property Changed_TEST2'}];
        this.dataSource.PartData.ErrorDescriptionGroup = [{name:'Optional'}, {name:'Part Number'}, {name:'PCB Footprint'}, {name:'Value'}];

        //p-multiSelect內容(ex：Page)一定要使用包含name屬性的物件
        this.dataSource.NetData.NetDiffs = [
          {objID: '2000', net_Pin_Name: 'VTT_DDR', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, error_CategoryForMultipleSelect: {name: 'Missing Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR1'}, design1: 'O', design2:'X', comment: 'D4C0102.1'},
          {objID: '2001', net_Pin_Name: 'GND', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, error_CategoryForMultipleSelect: {name: 'Missing Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR2'}, design1: 'O', design2:'X', comment: 'D4C0102.2;GC0103.2;GC0104.2'},
          {objID: '2002', net_Pin_Name: '+3V_DVI', design1PageForMultipleSelect: {name:'PAGE3'}, design2PageForMultipleSelect: {name:'PAGE4'}, error_CategoryForMultipleSelect: {name: 'Missing Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR3'}, design1: 'O', design2:'X', comment: 'GC0103.1;GC0104.1'},
          {objID: '2003', net_Pin_Name: 'GND', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, error_CategoryForMultipleSelect: {name: 'Adding Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR4'}, design1: 'X', design2:'O', comment: 'GQ0103.2;GQ0104.2'},
          {objID: '4003', net_Pin_Name: '+3V_DVI', design1PageForMultipleSelect: {name:'PAGE1'}, design2PageForMultipleSelect: {name:'PAGE2'}, error_CategoryForMultipleSelect: {name: 'Adding Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR4'}, design1: 'X', design2:'O', comment: 'GQ0103.1;GQ0104.1'}
        ];  
           
        this.dataSource.NetData.Design1PageGroup =  this.getDistinct(this.dataSource.NetData.NetDiffs.map(c => c.design1PageForMultipleSelect));
        this.dataSource.NetData.Design2PageGroup =  this.getDistinct(this.dataSource.NetData.NetDiffs.map(c => c.design2PageForMultipleSelect));
        this.dataSource.NetData.ErrorCategoryGroup = this.getDistinct(this.dataSource.NetData.NetDiffs.map(c => c.error_CategoryForMultipleSelect));
        this.dataSource.NetData.ErrorDescriptionGroup = this.getDistinct(this.dataSource.NetData.NetDiffs.map(c => c.error_DescriptionForMultipleSelect));
    }

    //subscribe upload event
    this.uploadInfoProcessing(this.uploadDsn1, EnumFileType.Source);
    this.uploadInfoProcessing(this.uploadDsn2, EnumFileType.Destination);

    //subscribe window width change event    
    this.changeModeInfo();

    this.form = this._formBuilder.group({
      CompareMode : ['0', Validators.required],
      DsnSourceFile: ['', Validators.required],
      DsnDestinationFile: ['', Validators.required]
    })
  }

  @HostListener('window:message', ['$event'])
  onMessage(event: MessageEvent): void
  {
    console.log(`[DRA]get postMessage from ${event.data.name}：`);
    console.log(event.data);

    if (this._postMessageService.schematicViewerUrl.indexOf(event.origin) === -1)
    {//檢查postMessage data是否來自SchematicViewer
      return;
    }

    if (!(typeof (event.data) === "object")) 
    {
      return;
    }

    this._postMessageService.pmEvent = event;

    switch(this._postMessageService.receivePmData?.cmd)
    {
      case CmdType.register:        
        let pmPara: CompareErrorResult = 
        {
          comparePageMapping: this._postMessageService.receivePmData.name == this.compareErrorResult.designSN1 ? this.compareErrorResult.design1_2 : this.compareErrorResult.design2_1,
          designSN1: this.compareErrorResult.designSN1,
          designSN2: this.compareErrorResult.designSN2,
          netErrors: this.compareErrorResult.netErrors,
          partErrors: this.compareErrorResult.partErrors
        }
        if(this.w1 && this._postMessageService.receivePmData.name == pmPara.designSN1)
        {          
          this._postMessageService.register(this.w1, pmPara);
        }
        else if(this.w2 && this._postMessageService.receivePmData.name == pmPara.designSN2)
        {
          this._postMessageService.register(this.w2, pmPara);
        }
        break;
      case CmdType.moveToPageName:
      case CmdType.draw:
      case CmdType.searchResultClick:
      case CmdType.zoom:
      case CmdType.closePage:
      case CmdType.moveToPage:
      case CmdType.markPosition:
        this._postMessageService.broadcast(this._postMessageService.receivePmData?.name);
        break;
    }
  }

  /**
   * 監聽螢幕寬度，改變icon
   */
  @HostListener('window:resize', ['$event'])
  onWindowResize() 
  {
    this.changeModeInfo();
  }

  /**
   * 變更比對模式Radio Buttion
   * ref：https://www.tutsmake.com/angular-11-checkbox-checked-event-tutorial/#google_vignette
   * @param e 比對模式：0:one way(單向), 1:two way(雙向)
   */
  onCompareModeChange(e: any)
  {
    this.compareMode$.next(e.target.value);
    this.changeModeInfo();
  }

  /**
   * 變更比對模式Radio Buttion，共用Method
   * pipe with if condition, ref：https://www.reddit.com/r/learnjavascript/comments/bzxm1g/rxjs_how_to_conditionally_pipe_an_observer/
   */
  changeModeInfo()
  {
    this.compareMode$
    // .pipe(
    //   switchMap(value => {
    //     if (isInitial)
    //     {
    //       take(1);
    //     }
    //     return value;
    //   })
    // )
    .subscribe(mode => {

      this.compareMode = mode;
      this.cnCompareMode = mode == "0" ? "單向" : "雙向";
  
      if (window.innerWidth < 768)
      {//螢幕寬度小於768px
        this.compareModeIcon = mode == "0" ? "fa fa-long-arrow-down" : "fa fa-arrows-v";
      }
      else 
      {
        this.compareModeIcon = mode == "0" ? "fa fa-long-arrow-right" : "fa fa-arrows-h";
      }
    })

  }

  /**
   * Upload元件訂閱，共用Method
   * @param res Upload元件訂閱回傳結果
   */
  uploadInfoProcessing(compoUpload:UploadComponent, dsnFileType: EnumFileType): void
  {
    let oldFileName:string = '';

    compoUpload?.uploadInfo$.pipe(
      concatMap(res => {
        if (res.uploadStatus == enumUploadStatus.Success)
        {    

          if (res.uploadFileInfo.message)
          {
            this._loadingService.setLoading(false);
            this._salService.showSwal("", res.uploadFileInfo.message, "error");
            return of(undefined);
          }

          oldFileName = res.uploadFileInfo.content.oldFileName;
          this._loadingService.setLoading(true, "檔案上傳完畢，開始進行轉檔...");
          let data:ToExp3File = {FileType: dsnFileType, UploadFileName: res.uploadFileInfo.content.newFileName};

          if (dsnFileType == EnumFileType.Source){
            this.sourceFileUploadMsg = '';
          }
          else{
            this.destinationFileUploadMsg = '';
          }

          return this._rdaService.ToExp3File(data);
        }
        else
        {
          if (res.uploadStatus == enumUploadStatus.None)
          {
            this._loadingService.setLoading(false);
            if (res.uploadFileInfo.message)
            {
              this._salService.showSwal("", `${res.uploadFileInfo.message}`, "warning");
            }            
          }
          else if (res.uploadStatus == enumUploadStatus.Start)
          {
            this._loadingService.setLoading(true, "檔案上傳中...");
          }
          else if (res.uploadStatus == enumUploadStatus.Fail)
          {
            this._loadingService.setLoading(false);
            this._salService.showSwal("", "上傳失敗.", "error");
          }

          if (dsnFileType == EnumFileType.Source){
            this.sourceFileUploadMsg = '';
          }
          else{
            this.destinationFileUploadMsg = '';
          }

          return of(undefined);
        }

      }))
      .subscribe(res => { 
        this._loadingService.setLoading(false);
        
        if (res != undefined)
        {        
          if (res.success)
          {
            if (dsnFileType == EnumFileType.Source)
            {     
              this.oldSourceFileName = oldFileName;
              this.sourceFileUploadMsg = `${oldFileName}分頁取得完成 @${moment().format('YYYY/MM/DD HH:mm:ss')}`         
              this.form.controls[FormControlName.inputDsnSourceFileName].setValue(res.content);
              this.sourceSchematic.dataSource.data = res.content.schematicDatas;
              this.sourceSchematic.expandAll();
              this.sourceId = '';
            }
            else 
            {
              this.oldDistinationFileName = oldFileName;
              this.destinationFileUploadMsg = `${oldFileName}分頁取得完成 @${moment().format('YYYY/MM/DD HH:mm:ss')}`           
              this.form.controls[FormControlName.inputDsnDestinationFileName].setValue(res.content);
              this.destinationSchematic.dataSource.data = res.content.schematicDatas;
              this.destinationSchematic.expandAll();
              this.destId = '';
            }

            let msg = "分頁取得完成.";
            if (res.message)
            {
              msg += "<br\><br\>" + res.message;
            }

            this._salService.showSwal("", msg, "success");
          }
          else
          {
            this._salService.showSwal("", res.message, "error");

            let nullDataSource : TodoItemNode[] = []
            if (dsnFileType == EnumFileType.Source)
            {     
              this.sourceFileUploadMsg = `${oldFileName}分頁取得失敗 @${moment().format('YYYY/MM/DD HH:mm:ss')}`
              this.form.controls[FormControlName.inputDsnSourceFileName].setValue("");         
              this.sourceSchematic.dataSource.data = nullDataSource;
            }
            else 
            {
              this.destinationFileUploadMsg = `${oldFileName}分頁取得失敗 @${moment().format('YYYY/MM/DD HH:mm:ss')}`   
              this.form.controls[FormControlName.inputDsnDestinationFileName].setValue("");        
              this.destinationSchematic.dataSource.data = nullDataSource;
            }
          }  
          
          this.sourceSchematic.checklistSelection.clear();
          this.destinationSchematic.checklistSelection.clear();
        }
    });
  }

  /*
   *線路圖比對
   */
   submit()
   {
    //取得選取分頁
    var sourceSchematicSelectedNode = this.sourceSchematic.checklistSelection.selected.filter(c => c.level == 1);
    var destinationSchematicSelectedNode = this.destinationSchematic.checklistSelection.selected.filter(c => c.level == 1);
    console.log(this.form?.value);

    let data:ReqDsnCompare = {
      CompareMode:this.form.controls[FormControlName.inputCompareMode].value,
      SelectedSourceSchematicPage:sourceSchematicSelectedNode,
      SelectedDestinationSchematicPage:destinationSchematicSelectedNode
    }

    if (this.sourceId)
    {
      data.SourceId = this.sourceId;
    }

    if (this.destId)
    {
      data.DestId = this.destId;
    }


    if (data.SelectedSourceSchematicPage.length <= 0 || data.SelectedDestinationSchematicPage.length <= 0)
    {
      this._salService.showSwal("", "請選擇要進行比對之分頁.", "error");
      return;
    }

    this._loadingService.setLoading(true, "開始進行線路圖比對...");

    //關閉舊的Schematic Viewer視窗
    this._postMessageService.close(true);

    this._rdaService.DsnCompare(data).subscribe(
      {next: res => 
        {
          this.compareErrorResult = 
          {
            designSN1: res.designSN1,
            designSN2: res.designSN2,
            design1_2: res.designDiff.design1_2,
            design2_1: res.designDiff.design2_1,
            partErrors: res.designDiff.partDiffs,
            netErrors: res.designDiff.netDiffs
          }

          /***Setting Part Compare Object***/ 
          res.designDiff.partDiffs.forEach(c => {          
            //p-multiSelect轉物件
              c.design1PageForMultipleSelect = {name : c.design1Page};
              c.design2PageForMultipleSelect = {name : c.design2Page};
              c.error_CategoryForMultipleSelect = {name: c.error_Category};                
              c.error_DescriptionForMultipleSelect = {name: c.error_Description};     
          });
          this.dataSource.PartData.Design1PageGroup =  this.getDistinct(res.designDiff.partDiffs.map(c => c.design1PageForMultipleSelect));
          this.dataSource.PartData.Design2PageGroup =  this.getDistinct(res.designDiff.partDiffs.map(c => c.design2PageForMultipleSelect));
          this.dataSource.PartData.ErrorCategoryGroup = this.getDistinct(res.designDiff.partDiffs.map(c => c.error_CategoryForMultipleSelect));
          this.dataSource.PartData.ErrorDescriptionGroup = this.getDistinct(res.designDiff.partDiffs.map(c => c.error_DescriptionForMultipleSelect));
          this.dataSource.PartData.PartDiffs = res.designDiff.partDiffs;



          /***Setting Net Compare Object***/        
          res.designDiff.netDiffs.forEach(c => {
            //p-multiSelect轉物件               
            c.design1PageForMultipleSelect = {name : c.design1Page};                        
            c.design2PageForMultipleSelect = {name : c.design2Page};              
            c.error_CategoryForMultipleSelect = {name: c.error_Category};                                
            c.error_DescriptionForMultipleSelect = {name: c.error_Description};             
          });
          this.dataSource.NetData.Design1PageGroup =  this.getDistinct(res.designDiff.netDiffs.map(c => c.design1PageForMultipleSelect));
          this.dataSource.NetData.Design2PageGroup =  this.getDistinct(res.designDiff.netDiffs.map(c => c.design2PageForMultipleSelect));
          this.dataSource.NetData.ErrorCategoryGroup = this.getDistinct(res.designDiff.netDiffs.map(c => c.error_CategoryForMultipleSelect));//p-table header的下拉filter內容
          this.dataSource.NetData.ErrorDescriptionGroup = this.getDistinct(res.designDiff.netDiffs.map(c => c.error_DescriptionForMultipleSelect));//p-table header的下拉filter內容
          this.dataSource.NetData.NetDiffs = res.designDiff.netDiffs;//p-table的欄位內容

          //console.log(this.dataSource.PartData);
          //console.log(this.dataSource.NetData);
          this._loadingService.setLoading(false);      

          

          let width = window.screen.availWidth/2 ;
          let height = window.screen.availHeight - 65;
          let w1Prop: SchematicViewerWindow = { rdcsSN: res.designSN1, designName: this.oldSourceFileName, darType: this.sourceId ? 'orcad' : 'source', width: width, height: height, dualScreenLeft: 0} 
          let w2Prop: SchematicViewerWindow = { rdcsSN: res.designSN2, designName: this.oldDistinationFileName, darType: this.destId ? 'orcad' : 'destination', width: width, height: height, dualScreenLeft: width} 
          if(!this.PopupBlock(w1Prop, w2Prop))
          {
            let opt: SweetAlertOptions = {
              imageUrl: "/assets/browser_blocker.jpg",
            };
            this._salService.showSwal('', '請允許快顯視窗顯示.', 'warning', opt);
          }
          
        },
        error: (err) => {          
          this._loadingService.setLoading(false);
          this._salService.showSwal("", `比對發生異常，請聯繫LayoutCAE Team. ${err}`, "error");
        }
      });
   }  

   /**
    * 是否線路圖是否被快顯封鎖阻擋
    * @param w1Prop 
    * @param w2Prop 
    * @returns true:dar快顯封鎖關閉, false:dar快顯封鎖開啟
    */
   PopupBlock = (w1Prop: SchematicViewerWindow, w2Prop:SchematicViewerWindow) =>
   {
    this.w1 = window.open(
    `${environment.schematicViewerUrl}?rdcsSN=${w1Prop.rdcsSN}&designName=${w1Prop.designName}&consoleHost=${environment.apiBaseUrl}&darType=${w1Prop.darType}&fileId=${this.sourceId}`, 
    "_blank", 
    `scrollbars=1,toolbar=0,resizable=1,status=0,width=${w1Prop.width},height=${w1Prop.height},top=0,left=${w1Prop.dualScreenLeft},directores=0`
    ) ?? undefined;

    this.w2 = window.open(
    `${environment.schematicViewerUrl}?rdcsSN=${w2Prop.rdcsSN}&designName=${w2Prop.designName}&consoleHost=${environment.apiBaseUrl}&darType=${w2Prop.darType}&fileId=${this.destId}`, 
    "_blank", 
    `scrollbars=1,toolbar=0,resizable=1,status=0,width=${w2Prop.width},height=${w2Prop.height},top=0,left=${w2Prop.dualScreenLeft},directores=0`
   ) ?? undefined;   
   
    return (this.w1 !==null && this.w1 != undefined) && (this.w2 !==null && this.w2 != undefined);
   }

   /**
    * Clear DataTable Filter
    */
   clear(table: Table) 
   {
    if (this.compareErrorResult.partErrors <= 0 && this.compareErrorResult.netErrors <= 0)
    {
      this._salService.showSwal('', '查無比對資料.', 'error')      
      return;
    }

    this.filterCondition = { 
      PartData: { PartDiffs:[], Design1PageGroup:[], Design2PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] },
      NetData: { NetDiffs:[], Design1PageGroup:[], Design2PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] }
    };

    table.clear();
   }

   /**
    * Distinct Header的下拉選單內容
    */
   getDistinct(data: (MultipleSelectObj|undefined)[]): MultipleSelectObj[]
   {
     let res = new Map
     let arr: MultipleSelectObj[] = [];    
     for(let obj of data)
     {
         if(/*obj?.name &&*/ !res.has(obj?.name))
         {
           res.set(obj?.name??'', obj?.name??'');          
           arr.push(obj ?? {name:''});
         }   
     }    
 
     return [...new Set(arr)];
   }
   
   /**
    * 比對結果Excel下載
    * @param type 
    */
   onExcelDownload(type: string)
   {
    if (this.compareErrorResult.partErrors <= 0 && this.compareErrorResult.netErrors <= 0)
    {
      this._salService.showSwal('', '查無比對資料.', 'error')      
      return;
    }

    this._loadingService.setLoading(true, 'Excel下載...');
    let data: DesignDiff = { partDiffs:[], netDiffs:[], design1_2:[], design2_1:[], identity1:'', identity2:''}

    if (type.toLowerCase() == 'part')
    {
      data.netDiffs = [];
      data.partDiffs = 
      this.compareErrorResult.partErrors.filter((c: { design1Page: {} | undefined; design2Page: {} | undefined; error_Category: {} | undefined; error_Description: {} | undefined; }) => 
        (this.filterCondition.PartData.Design1PageGroup.length > 0 ? this.filterCondition.PartData.Design1PageGroup.includes(c.design1Page) : 1==1) &&
        (this.filterCondition.PartData.Design2PageGroup.length > 0 ? this.filterCondition.PartData.Design2PageGroup.includes(c.design2Page) : 1==1) &&
        (this.filterCondition.PartData.ErrorCategoryGroup.length > 0 ? this.filterCondition.PartData.ErrorCategoryGroup.includes(c.error_Category) : 1==1) &&
        (this.filterCondition.PartData.ErrorDescriptionGroup.length > 0 ?this.filterCondition.PartData.ErrorDescriptionGroup.includes(c.error_Description) : 1==1)
        );
    }
    else
    {
      data.partDiffs = [];
      data.netDiffs = 
      this.compareErrorResult.netErrors.filter((c: { design1Page: {} | undefined; design2Page: {} | undefined; error_Category: string | undefined; error_Description: string | undefined; }) => 
        (this.filterCondition.NetData.Design1PageGroup.length > 0 ? this.filterCondition.NetData.Design1PageGroup.includes(c.design1Page) : 1==1) &&
        (this.filterCondition.NetData.Design2PageGroup.length > 0 ? this.filterCondition.NetData.Design2PageGroup.includes(c.design2Page) : 1==1) &&
        (this.filterCondition.NetData.ErrorCategoryGroup.length > 0 ? this.filterCondition.NetData.ErrorCategoryGroup.includes(c.error_Category) : 1==1) &&
        (this.filterCondition.NetData.ErrorDescriptionGroup.length > 0 ? this.filterCondition.NetData.ErrorDescriptionGroup.includes(c.error_Description) : 1==1)
        );
    }

    this._rdaService.ExportCompareResult(data).subscribe({
      next: (res)=>
      {
        this._rdaService.downloadFile(
          `${type}CompareResult_${this.datepipe.transform(new Date(), "yyyyMMddHHmmss")}.xlsx`, 
          res, 
          'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
          );

        this._salService.showSwal("", "Excel下載完成.", "success");
        this._loadingService.setLoading(false);
      },
      error: (err: HttpErrorResponse)=>{
        let enc = new TextDecoder("utf-8");
        let arr = new Uint8Array(err?.error);  
        let decodeContent = enc.decode(arr);

        this._salService.showSwal("", `Excel下載失敗。${decodeContent}.`, "error");
        this._loadingService.setLoading(false);
      }
    });
   }

/** 
 * 取得(Part)篩選條件 for Excel下載
 * @param category 
 * @param data 
 */
  onChangePartFilterCondition(category:string, data:any)
  {    
      let arrayData = data.map((c: { name: string; }) => c.name);

      switch (category)
      {
        case "Design1PageGroup":
          this.filterCondition.PartData.Design1PageGroup = arrayData;      
          break;
        case "Design2PageGroup":
          this.filterCondition.PartData.Design2PageGroup = arrayData;  
          break;
        case "ErrorCategoryGroup":
          this.filterCondition.PartData.ErrorCategoryGroup = arrayData;   
          break;
        case "ErrorDescriptionGroup":
          this.filterCondition.PartData.ErrorDescriptionGroup = arrayData;    
          break;
      }    
      //console.log(this.filterCondition.PartData);
  }

/**
 * 取得(Net)篩選條件 for Excel下載 
 * @param category 
 * @param data 
 */
  onChangeNetFilterCondition(category:string, data:any)
  {    
    let arrayData = data.map((c: { name: string; }) => c.name);

    switch (category)
    {
      case "Design1PageGroup":
        this.filterCondition.NetData.Design1PageGroup = arrayData;      
        break;
      case "Design2PageGroup":
        this.filterCondition.NetData.Design2PageGroup = arrayData;  
        break;
      case "ErrorCategoryGroup":
        this.filterCondition.NetData.ErrorCategoryGroup = arrayData;   
        break;
      case "ErrorDescriptionGroup":
        this.filterCondition.NetData.ErrorDescriptionGroup = arrayData;    
        break;
    }    
    //console.log(this.filterCondition.NetData);
  }

   /**
    * lastValueFrom test
    * @param data 
    */
  // async CompareAsync(data: ReqDsnCompare)
  // {
  //   const qq$ = this._rdaService.DsnCompare(data);
  //   const listQQ$ = of(qq$,qq$,qq$);
  //   const resCompare = await lastValueFrom(qq$);

  //   this.compareErrorResult = 
  //   {
  //     designSN1: resCompare.designSN1,
  //     designSN2: resCompare.designSN2,
  //     design1_2: resCompare.designDiff.design1_2,
  //     design2_1: resCompare.designDiff.design2_1,
  //     partErrors: resCompare.designDiff.partDiffs,
  //     netErrors: resCompare.designDiff.netDiffs
  //   }   
  //  }  
}

class FormControlName
{  
  public static readonly inputCompareMode: string = "CompareMode";
  public static readonly inputDsnSourceFileName: string = "DsnSourceFile";
  public static readonly inputDsnDestinationFileName: string = "DsnDestinationFile";
}

