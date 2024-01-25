import { NetCompareData, PageMapping, PartCompareData } from "./dto/response/res-dsn-compare";

export interface CompareErrorResult
{
    designSN1?: string;
    designSN2?: string;
    design1_2?: PageMapping[];
    design2_1?: PageMapping[];
    partErrors?: PartCompareData[] | any;
    netErrors?: NetCompareData[] | any;
    comparePageMapping?: PageMapping[];
}