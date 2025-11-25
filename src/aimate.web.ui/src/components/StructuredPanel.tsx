import { useState } from "react";
import { Button } from "./ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "./ui/table";
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "./ui/card";
import { ArrowUpDown, ChevronLeft, ChevronRight, ChevronDown, ChevronUp } from "lucide-react";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Textarea } from "./ui/textarea";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "./ui/alert-dialog";

interface StructuredAction {
  type: string;
  title: string;
  tool?: string;
  args?: Record<string, any>;
}

interface StructuredData {
  type: "panel.table" | "panel.list" | "panel.form" | "panel.kv";
  title: string;
  columns?: string[];
  rows?: any[][];
  items?: Array<{ title: string; content: string }>;
  fields?: Array<{ name: string; label: string; type: string; value: string; required?: boolean }>;
  kvPairs?: Array<{ key: string; value: string }>;
  rowActions?: StructuredAction[];
  actions?: StructuredAction[];
  submit?: StructuredAction;
  onSuccess?: any;
}

interface StructuredPanelProps {
  data: StructuredData;
  onAction?: (action: StructuredAction, rowData?: any[] | Record<string, any>) => void;
}

export function StructuredPanel({ data, onAction }: StructuredPanelProps) {
  if (data.type === "panel.table") {
    return <StructuredTable data={data} onAction={onAction} />;
  }
  if (data.type === "panel.list") {
    return <StructuredList data={data} onAction={onAction} />;
  }
  if (data.type === "panel.form") {
    return <StructuredForm data={data} onAction={onAction} />;
  }
  if (data.type === "panel.kv") {
    return <StructuredKV data={data} onAction={onAction} />;
  }

  return null;
}

function StructuredTable({ data, onAction }: StructuredPanelProps) {
  const [sortColumn, setSortColumn] = useState<number | null>(null);
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 5;

  const handleSort = (columnIndex: number) => {
    if (sortColumn === columnIndex) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortColumn(columnIndex);
      setSortDirection("asc");
    }
  };

  // Sort data
  let sortedRows = [...(data.rows || [])];
  if (sortColumn !== null) {
    sortedRows.sort((a, b) => {
      const aVal = a[sortColumn];
      const bVal = b[sortColumn];
      
      if (typeof aVal === "number" && typeof bVal === "number") {
        return sortDirection === "asc" ? aVal - bVal : bVal - aVal;
      }
      
      const aStr = String(aVal).toLowerCase();
      const bStr = String(bVal).toLowerCase();
      
      if (sortDirection === "asc") {
        return aStr.localeCompare(bStr);
      } else {
        return bStr.localeCompare(aStr);
      }
    });
  }

  // Paginate data
  const totalPages = Math.ceil(sortedRows.length / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedRows = sortedRows.slice(startIndex, endIndex);

  const handleRowAction = (action: StructuredAction, row: any[]) => {
    // Replace $row[0] placeholders with actual row values
    const processedAction = {
      ...action,
      args: action.args ? Object.fromEntries(
        Object.entries(action.args).map(([key, value]) => {
          if (typeof value === 'string' && value.startsWith('$row[')) {
            const index = parseInt(value.match(/\$row\[(\d+)\]/)?.[1] || '0');
            return [key, row[index]];
          }
          return [key, value];
        })
      ) : undefined
    };

    onAction?.(processedAction, row);
  };

  const handleGlobalAction = (action: StructuredAction) => {
    onAction?.(action);
  };

  const handlePageChange = (newPage: number) => {
    setCurrentPage(Math.max(1, Math.min(newPage, totalPages)));
  };

  return (
    <Card className="my-3 border-purple-200 dark:border-purple-800/30 w-full">
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">{data.title}</CardTitle>
          {data.actions && data.actions.length > 0 && (
            <div className="flex gap-2">
              {data.actions.map((action, idx) => (
                <Button
                  key={idx}
                  size="sm"
                  variant="default"
                  onClick={() => handleGlobalAction(action)}
                  className="cursor-pointer"
                >
                  {action.title}
                </Button>
              ))}
            </div>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <div className="rounded-md border border-gray-200 dark:border-gray-800">
          <Table>
            <TableHeader>
              <TableRow>
                {(data.columns || []).map((column, idx) => (
                  <TableHead key={idx}>
                    <button
                      onClick={() => handleSort(idx)}
                      className="flex items-center gap-2 cursor-pointer hover:text-purple-600 dark:hover:text-purple-400 transition-colors"
                    >
                      {column}
                      <ArrowUpDown className="h-3 w-3" />
                    </button>
                  </TableHead>
                ))}
                {data.rowActions && data.rowActions.length > 0 && (
                  <TableHead className="text-right">Actions</TableHead>
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {paginatedRows.map((row, rowIdx) => (
                <TableRow key={rowIdx}>
                  {row.map((cell, cellIdx) => (
                    <TableCell key={cellIdx}>{cell}</TableCell>
                  ))}
                  {data.rowActions && data.rowActions.length > 0 && (
                    <TableCell className="text-right">
                      <div className="flex gap-2 justify-end">
                        {data.rowActions.map((action, actionIdx) => (
                          <Button
                            key={actionIdx}
                            size="sm"
                            variant="ghost"
                            onClick={() => handleRowAction(action, row)}
                            className="cursor-pointer"
                          >
                            {action.title}
                          </Button>
                        ))}
                      </div>
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </CardContent>
      {totalPages > 1 && (
        <CardFooter className="flex items-center justify-between pt-3">
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Showing {startIndex + 1} to {Math.min(endIndex, sortedRows.length)} of {sortedRows.length} results
          </div>
          <div className="flex items-center gap-2">
            <Button
              size="sm"
              variant="outline"
              onClick={() => handlePageChange(currentPage - 1)}
              disabled={currentPage === 1}
              className="cursor-pointer"
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <span className="text-sm">
              Page {currentPage} of {totalPages}
            </span>
            <Button
              size="sm"
              variant="outline"
              onClick={() => handlePageChange(currentPage + 1)}
              disabled={currentPage === totalPages}
              className="cursor-pointer"
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </CardFooter>
      )}
    </Card>
  );
}

function StructuredList({ data, onAction }: StructuredPanelProps) {
  const [expandedItems, setExpandedItems] = useState<Set<number>>(new Set());

  const toggleItem = (index: number) => {
    const newExpanded = new Set(expandedItems);
    if (newExpanded.has(index)) {
      newExpanded.delete(index);
    } else {
      newExpanded.add(index);
    }
    setExpandedItems(newExpanded);
  };

  return (
    <Card className="my-3 border-purple-200 dark:border-purple-800/30 w-full">
      <CardHeader className="pb-3">
        <CardTitle className="text-lg">{data.title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          {(data.items || []).map((item, index) => (
            <div
              key={index}
              className="border border-gray-200 dark:border-gray-800 rounded-lg overflow-hidden"
            >
              <button
                onClick={() => toggleItem(index)}
                className="w-full p-3 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors cursor-pointer"
              >
                <div className="flex items-center gap-2">
                  <span className="text-purple-500">•</span>
                  <span className="font-medium">{item.title}</span>
                </div>
                {expandedItems.has(index) ? (
                  <ChevronUp className="h-4 w-4 text-gray-500" />
                ) : (
                  <ChevronDown className="h-4 w-4 text-gray-500" />
                )}
              </button>
              {expandedItems.has(index) && (
                <div className="px-3 pb-3 pt-1 text-sm text-gray-600 dark:text-gray-400 border-t border-gray-200 dark:border-gray-800">
                  {item.content}
                </div>
              )}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function StructuredKV({ data, onAction }: StructuredPanelProps) {
  return (
    <Card className="my-3 border-purple-200 dark:border-purple-800/30 w-full">
      <CardHeader className="pb-3">
        <CardTitle className="text-lg">{data.title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="rounded-md border border-gray-200 dark:border-gray-800">
          <Table>
            <TableBody>
              {(data.kvPairs || []).map((pair, index) => (
                <TableRow key={index}>
                  <TableCell className="font-medium w-1/3">{pair.key}</TableCell>
                  <TableCell>{pair.value}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </CardContent>
    </Card>
  );
}

function StructuredForm({ data, onAction }: StructuredPanelProps) {
  const [formData, setFormData] = useState<Record<string, any>>(
    (data.fields || []).reduce((acc, field) => {
      acc[field.name] = field.value;
      return acc;
    }, {} as Record<string, any>)
  );
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const handleChange = (name: string, value: any) => {
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = () => {
    if (data.submit) {
      onAction?.(data.submit, formData);
    }
  };

  const handleDelete = () => {
    onAction?.({ type: "action.delete", title: "Delete", tool: "delete" }, formData);
    setDeleteDialogOpen(false);
  };

  return (
    <>
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete this record.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel className="cursor-pointer">Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="cursor-pointer bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Card className="my-3 border-purple-200 dark:border-purple-800/30 w-full">
        <CardHeader className="pb-3">
          <CardTitle className="text-lg">{data.title}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {(data.fields || []).map((field, index) => (
              <div key={index} className="space-y-2">
                <Label htmlFor={field.name}>
                  {field.label}
                  {field.required && <span className="text-red-500 ml-1">*</span>}
                </Label>
                {field.type === "textarea" ? (
                  <Textarea
                    id={field.name}
                    value={formData[field.name] || ""}
                    onChange={(e) => handleChange(field.name, e.target.value)}
                    className="w-full"
                  />
                ) : (
                  <Input
                    id={field.name}
                    type={field.type}
                    value={formData[field.name] || ""}
                    onChange={(e) => handleChange(field.name, e.target.value)}
                    className="w-full"
                  />
                )}
              </div>
            ))}
          </div>
        </CardContent>
        <CardFooter className="flex justify-between border-t border-gray-200 dark:border-gray-800 pt-4">
          <Button
            variant="destructive"
            onClick={() => setDeleteDialogOpen(true)}
            className="cursor-pointer"
          >
            Delete
          </Button>
          <Button onClick={handleSubmit} className="cursor-pointer">
            Save
          </Button>
        </CardFooter>
      </Card>
    </>
  );
}
