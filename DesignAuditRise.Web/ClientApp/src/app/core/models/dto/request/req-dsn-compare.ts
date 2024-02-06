import { TodoItemFlatNode } from '../../../../shared/ng-material/checkbox-tree/checkbox-tree.component';

export interface ReqDsnCompare {
    CompareMode: string
    //SourceSchematic: TodoItemNode[],
    //DestinationSchematic: TodoItemNode[],
    SourceId?: string
    DestId?: string
    SelectedSourceSchematicPage: TodoItemFlatNode[]
    SelectedDestinationSchematicPage: TodoItemFlatNode[]
  }
