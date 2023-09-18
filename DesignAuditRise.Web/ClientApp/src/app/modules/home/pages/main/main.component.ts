import { PartCompareData, NetCompareData, MultipleSelectObj } from './../../../shared/models/dto/response/res-dsn-compare';
import { ToExp3File, EnumFileType } from './../../../shared/models/dto/request/to-exp-3file';
import { ReqDsnCompare } from '../../../shared/models/dto/request/req-dsn-compare';
import { enumUploadStatus, UploadComponent } from './../../../shared/components/upload/upload.component';
import { SweetAlertService } from './../../../core/service/sweet-alert.service';
import { RdaService } from './../../../core/http/rda.service';
import { CheckboxTreeComponent, TodoItemNode } from './../../../shared/ng-material/checkbox-tree/checkbox-tree.component';
import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators} from '@angular/forms';
import { BehaviorSubject, concatMap, of } from 'rxjs';
import { LoadingService } from 'src/app/modules/shared/components/loading/loading.service';
import * as moment from 'moment';
import { TableDataSource } from 'src/app/modules/shared/models/dto/table-data-source';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})

export class MainComponent implements OnInit 
{
  @ViewChild('sourceSchematic', { static: true }) sourceSchematic!: CheckboxTreeComponent;
  @ViewChild('destinationSchematic', { static: true }) destinationSchematic!: CheckboxTreeComponent;
  @ViewChild('uploadDsn1', { static: true }) uploadDsn1!: UploadComponent;
  @ViewChild('uploadDsn2', { static: true }) uploadDsn2!: UploadComponent;

  form!:FormGroup;
  cnCompareMode: string = "";
  compareModeIcon: string = "";
  compareMode: string = "";
  compareMode$ = new BehaviorSubject("0");
  sourceFileUploadMsg:string="";
  destinationFileUploadMsg:string="";
  active:number = 1;
  dataSource: TableDataSource = { 
    PartData: { PartDiffs:[], PageGroup:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] },
    NetData: { NetDiffs:[], ErrorCategoryGroup:[], ErrorDescriptionGroup:[] }
  };
  selectedPartDatas!: PartCompareData;
  selectedNetDatas!: NetCompareData;
  oldSourceFileName:string='';
  oldDistinationFileName:string='';

  constructor
  (
    private _rdaService: RdaService,
    private _salService: SweetAlertService,
    private _loadingService: LoadingService,
    private _formBuilder: FormBuilder
  ){

  }     
  
  // getDistinct<T, K extends keyof T>(data: T[], property: K): T[K][] {
  //   const allValues = data.reduce((values: T[K][], current) => {
  //     if (current[property]) {
  //       values.push(current[property]);
  //     }
  //     return values;
  //   }, []);
  
  //   return [...new Set(allValues)];
  // }

  getDistinct(data: (MultipleSelectObj|undefined)[]): MultipleSelectObj[]
  {
    let res = new Map
    let arr: MultipleSelectObj[] = [];    
    for(let obj of data)
    {
        if(obj?.name && !res.has(obj?.name))
        {
          res.set(obj?.name, obj?.name);
          arr.push(obj);
        }   
    }    

    return [...new Set(arr)];
  }

  ngOnInit(): void
  {  
    let initialTreeData:TodoItemNode[] = [];
    this.sourceSchematic.dataSource.data = initialTreeData;
    this.destinationSchematic.dataSource.data = initialTreeData;

    if(true)
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
          {objID: '1000', schematic: 'SCHEMATIC11', pageForMultipleSelect: {name:'PAGE1'}, reference: 'D4C0104', part_Reference: 'D4C0104', error_CategoryForMultipleSelect: {name:'Property Changed1'}, error_DescriptionForMultipleSelect: {name:'Optional'}, design1: 'mbs_c0603', design2:'mbs_c0201_p0402', comment: '100NF/16V'},
          {objID: '1001', schematic: 'SCHEMATIC22', pageForMultipleSelect: {name:'PAGE1'}, reference: 'D4C0101', part_Reference: 'D4C0101', error_CategoryForMultipleSelect: {name:'Property Changed2'}, error_DescriptionForMultipleSelect: {name:'Part Number'}, design1: '/X/DDR4', design2:'/DDR4', comment: ''},
          {objID: '1002', schematic: 'SCHEMATIC33', pageForMultipleSelect: {name:'PAGE2'}, reference: 'D4C0101', part_Reference: 'D4C0101', error_CategoryForMultipleSelect: {name:'Property Changed3'}, error_DescriptionForMultipleSelect: {name:'PCB Footprint'}, design1: '11V233475150', design2:'11202V0027D000', comment: ''},
          {objID: '1003', schematic: 'SCHEMATIC44', pageForMultipleSelect: {name:'PAGE3'}, reference: 'D4C0101', part_Reference: 'D4C0101', error_CategoryForMultipleSelect: {name:'Property Changed4'}, error_DescriptionForMultipleSelect: {name:'Value'}, design1: '4.7UF/6.3V', design2:'100NF/16V', comment: ''}
        ];  
        
        this.dataSource.PartData.PageGroup = [{name:"PAGE1"}, {name:"PAGE2"}, {name:'PAGE3'}];
        this.dataSource.PartData.ErrorCategoryGroup = [{name:'Property Changed'}, {name:'Property Changed_TEST2'}];
        this.dataSource.PartData.ErrorDescriptionGroup = [{name:'Optional'}, {name:'Part Number'}, {name:'PCB Footprint'}, {name:'Value'}];

        //p-multiSelect內容(ex：Page)一定要使用包含name屬性的物件
        this.dataSource.NetData.NetDiffs = [
          {objID: '2000', net_Pin_Name: 'VTT_DDR', error_CategoryForMultipleSelect: {name: 'Missing Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR1'}, design1: 'O', design2:'X', comment: 'D4C0102.1'},
          {objID: '2001', net_Pin_Name: 'GND', error_CategoryForMultipleSelect: {name: 'Missing Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR2'}, design1: 'O', design2:'X', comment: 'D4C0102.2;GC0103.2;GC0104.2'},
          {objID: '2002', net_Pin_Name: '+3V_DVI', error_CategoryForMultipleSelect: {name: 'Missing Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR3'}, design1: 'O', design2:'X', comment: 'GC0103.1;GC0104.1'},
          {objID: '2003', net_Pin_Name: 'GND', error_CategoryForMultipleSelect: {name: 'Adding Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR4'}, design1: 'X', design2:'O', comment: 'GQ0103.2;GQ0104.2'},
          {objID: '4003', net_Pin_Name: '+3V_DVI', error_CategoryForMultipleSelect: {name: 'Adding Connections'}, error_DescriptionForMultipleSelect: {name: 'ERROR4'}, design1: 'X', design2:'O', comment: 'GQ0103.1;GQ0104.1'}
        ];  
            
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
   * 
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
    this.sourceFileUploadMsg = '';
    this.destinationFileUploadMsg = '';
    let oldFileName:string = '';

    compoUpload?.uploadInfo$.pipe(
      concatMap(res => {
        if (res.uploadStatus == enumUploadStatus.Success)
        {    
          oldFileName = res.uploadFileInfo.content.oldFileName;
          this._loadingService.setLoading(true, "檔案上傳完畢，開始進行轉檔...");
          let data:ToExp3File = {FileType: dsnFileType, UploadFileName: res.uploadFileInfo.content.newFileName};
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
              this.sourceSchematic.dataSource.data = res.content;
            }
            else 
            {
              this.oldDistinationFileName = oldFileName;
              this.destinationFileUploadMsg = `${oldFileName}分頁取得完成 @${moment().format('YYYY/MM/DD HH:mm:ss')}`           
              this.form.controls[FormControlName.inputDsnDestinationFileName].setValue(res.content);
              this.destinationSchematic.dataSource.data = res.content;
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
        }
    });
  }

  /*
   *開始比對
   */
   submit()
   {
     //取得選取分頁
     var sourceSchematicSelectedNode = this.sourceSchematic.checklistSelection.selected;
     var destinationSchematicSelectedNode = this.destinationSchematic.checklistSelection.selected;
     console.log(this.form?.value);
 
     let data:ReqDsnCompare = {
       CompareMode:this.form.controls[FormControlName.inputCompareMode].value,
       SelectedSourceSchematicPage:sourceSchematicSelectedNode,
       SelectedDestinationSchematicPage:destinationSchematicSelectedNode
     }
 
     if (data.SelectedSourceSchematicPage.length <= 0 || data.SelectedDestinationSchematicPage.length <= 0)
     {
       this._salService.showSwal("", "請選擇要進行比對之分頁.", "error");
       return;
     }
 
     this._rdaService.DsnCompare(data).subscribe(res => {

      /***Setting Part Compare Object***/ 
      res.partDiffs.forEach(c => {
        debugger;
        //p-multiSelect轉物件
        if(c.page)
        {
          c.pageForMultipleSelect = {name : c.page};
        }

        if (c.error_Category){
          c.error_CategoryForMultipleSelect = {name: c.error_Category};
        }
                
        if (c.error_Description){
          c.error_DescriptionForMultipleSelect = {name: c.error_Description};     
        }
      });
      this.dataSource.PartData.PageGroup =  this.getDistinct(res.partDiffs.map(c => c.pageForMultipleSelect));
      this.dataSource.PartData.ErrorCategoryGroup = this.getDistinct(res.partDiffs.map(c => c.error_CategoryForMultipleSelect));
      this.dataSource.PartData.ErrorDescriptionGroup = this.getDistinct(res.partDiffs.map(c => c.error_DescriptionForMultipleSelect));
      this.dataSource.PartData.PartDiffs = res.partDiffs;
      console.log(this.dataSource.PartData);



      /***Setting Net Compare Object***/        
      res.netDiffs.forEach(c => {
        //p-multiSelect轉物件
        if (c.error_Category){
          c.error_CategoryForMultipleSelect = {name: c.error_Category};
        }
                
        if (c.error_Description){
          c.error_DescriptionForMultipleSelect = {name: c.error_Description};     
        }
      });
      this.dataSource.NetData.ErrorCategoryGroup = this.getDistinct(res.netDiffs.map(c => c.error_CategoryForMultipleSelect));
      this.dataSource.NetData.ErrorDescriptionGroup = this.getDistinct(res.netDiffs.map(c => c.error_DescriptionForMultipleSelect));
      this.dataSource.NetData.NetDiffs = res.netDiffs;
     });
   }  

   /**
    * Clear DataTable Filter
    */
   clear(table: Table) 
   {
    table.clear();
   }
   
   excelDownload()
   {
    this._salService.showSwal("", "Excel Downloaded.", "success");
   }
}

class FormControlName
{  
  public static readonly inputCompareMode: string = "CompareMode";
  public static readonly inputDsnSourceFileName: string = "DsnSourceFile";
  public static readonly inputDsnDestinationFileName: string = "DsnDestinationFile";
}

