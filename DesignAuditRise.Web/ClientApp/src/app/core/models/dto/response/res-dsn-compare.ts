
export interface ResDsnComePare
{   
  designSN1: string;
  designSN2: string;
  designDiff: DesignDiff; 
}

export interface DesignDiff
{
  partDiffs: PartCompareData[];
  netDiffs: NetCompareData[];
  design1_2: PageMapping[];
  design2_1: PageMapping[];
  identity1: string;
  identity2: string;
}

export interface PageMapping
{
  source: string;
  target: string;
  comment: string;
}

export interface PartCompareData
{
    objID?: string;
    schematic?: string;
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
    design1PageForMultipleSelect?: MultipleSelectObj;
    design2Page?: string;  
    design2PageForMultipleSelect?: MultipleSelectObj;
}

export interface NetCompareData
{
  objID?:string;
  schematic?:string;
  page?:string
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
  design1PageForMultipleSelect?: MultipleSelectObj;
  design2Page?:string;
  design2PageForMultipleSelect?: MultipleSelectObj;
}

export interface MultipleSelectObj{
  name?: string;
}