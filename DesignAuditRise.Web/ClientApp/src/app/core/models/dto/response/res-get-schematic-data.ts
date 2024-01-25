import { TodoItemNode } from '../../../../shared/ng-material/checkbox-tree/checkbox-tree.component';

export interface ResGetSchematicData
{   
  Source: Schematic[];
  Destination: Schematic[];
}

interface Schematic
{
    item: string;
    children: TodoItemNode[];
}