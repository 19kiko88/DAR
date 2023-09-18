import { PartCompareData, NetCompareData, MultipleSelectObj } from './response/res-dsn-compare';

export interface TableDataSource 
{
    PartData: PartData
    NetData: NetData
}

interface PartData
{
    PartDiffs: PartCompareData[];
    PageGroup: ({}|undefined)[];
    ErrorCategoryGroup: ({}|undefined)[];
    ErrorDescriptionGroup: ({}|undefined)[];
}

interface NetData
{
    NetDiffs: NetCompareData[];
    ErrorCategoryGroup: (MultipleSelectObj)[];
    ErrorDescriptionGroup: (MultipleSelectObj)[];
}
