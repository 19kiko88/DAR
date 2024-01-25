import { PartCompareData, NetCompareData, MultipleSelectObj } from './dto/response/res-dsn-compare';

export interface TableDataSource 
{
    PartData: PartData
    NetData: NetData
}

interface PartData
{
    PartDiffs: PartCompareData[];
    Design1PageGroup: ({}|undefined)[];
    Design2PageGroup: ({}|undefined)[];
    ErrorCategoryGroup: ({}|undefined)[];
    ErrorDescriptionGroup: ({}|undefined)[];
}

interface NetData
{
    NetDiffs: NetCompareData[];
    Design1PageGroup: ({}|undefined)[];
    Design2PageGroup: ({}|undefined)[];
    ErrorCategoryGroup: ({}|undefined)[];
    ErrorDescriptionGroup: ({}|undefined)[];
}
