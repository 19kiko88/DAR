import { TodoItemFlatNode } from '../../../ng-material/checkbox-tree/checkbox-tree.component';

export interface ReqDsnCompare {
    CompareMode: string
    //SourceSchematic: TodoItemNode[],
    //DestinationSchematic: TodoItemNode[],
    SelectedSourceSchematicPage: TodoItemFlatNode[]
    SelectedDestinationSchematicPage: TodoItemFlatNode[]
  }
