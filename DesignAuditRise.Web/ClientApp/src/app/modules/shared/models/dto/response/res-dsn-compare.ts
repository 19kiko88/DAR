
export interface ResDsnComePare
{    
  partDiffs: PartCompareData[];
  netDiffs: NetCompareData[];    
}

export interface PartCompareData
{
    objID?: string;
    schematic?: string;
    pageForMultipleSelect?: MultipleSelectObj;
    page?:string
    reference?: string;
    part_Reference?: string;
    error_Category?: string;
    error_CategoryForMultipleSelect?: MultipleSelectObj;
    error_Description?:string;
    error_DescriptionForMultipleSelect?: MultipleSelectObj;
    design1?: string;
    design2?: string;
    comment?: string;
    design1ObjID?: string;
    design2ObjID?: string;
    design1Page?: string;
    design2Page?: string;  
}

export interface NetCompareData
{
  objID?:string;
  schematic?:string;
  page?:string
  pageForMultipleSelect?: MultipleSelectObj;
  pages_NetPinNames?: {[key: string]: string[];} //c# dictionary
  reference?:string;
  part_Reference?:string;
  net_Pin_Name?:string;
  design1PinName?:string;
  design1PinNumber?:string;
  design2PinName?:string;
  design2PinNumber?:string;
  error_Category?: string;
  error_CategoryForMultipleSelect?: MultipleSelectObj;
  error_Description?:string;
  error_DescriptionForMultipleSelect?: MultipleSelectObj;
  design1?:string;
  design2?:string;
  comment?:string;
  design1Schematic?:string;
  design2Schematic?:string;
  design1Page?:string;
  design2Page?:string;
}

export interface MultipleSelectObj{
  name: string;
}