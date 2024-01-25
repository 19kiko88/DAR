import { TodoItemNode } from '../../../../shared/ng-material/checkbox-tree/checkbox-tree.component';

export interface ResToExp3File
{   
  schematicDatas: Schematic[];
}

interface Schematic
{
    item: string;
    children: TodoItemNode[];
}