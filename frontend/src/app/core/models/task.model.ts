export type TaskPriority = 'Low' | 'Medium' | 'High';

export type TaskStatus = 'Todo' | 'Done';

export interface TaskItem {
  id: number;
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDate: string | null;
  status: TaskStatus;
}

export interface SaveTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDate: string | null;
}