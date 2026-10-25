"use client";

import { useMemo, useState } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Search, Filter } from "lucide-react";
import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  transactions,
  getTransactionSummary,
  formatCurrency,
} from "@/lib/financials/mock-data";
import type {
  Currency,
  Transaction,
  TransactionSortField,
  TransactionType,
  TransactionStatus,
  SortDirection,
} from "@/lib/financials/types";

interface TransactionsViewProps {
  currency: Currency;
}

const typeLabels: Record<TransactionType, string> = {
  energy_sale: "Energy Sale",
  energy_purchase: "Energy Purchase",
  demand_response: "Demand Response",
  capacity_payment: "Capacity Payment",
  ancillary_service: "Ancillary Service",
  rec_sale: "REC Sale",
  carbon_credit: "Carbon Credit",
  maintenance_cost: "Maintenance",
  grid_charge: "Grid Charge",
};

const statusColors: Record<TransactionStatus, string> = {
  completed: "bg-status-operational/15 text-status-operational border-status-operational/30",
  pending: "bg-severity-medium/15 text-severity-medium border-severity-medium/30",
  invoiced: "bg-primary/15 text-primary border-primary/30",
  overdue: "bg-severity-critical/15 text-severity-critical border-severity-critical/30",
};

function SortableHeader({
  label,
  field,
  sortField,
  sortDir,
  onSort,
}: {
  label: string;
  field: TransactionSortField;
  sortField: TransactionSortField;
  sortDir: SortDirection;
  onSort: (field: TransactionSortField) => void;
}) {
  const isActive = sortField === field;
  return (
    <button
      onClick={() => onSort(field)}
      className="flex items-center gap-1 font-medium hover:text-foreground transition-colors"
    >
      {label}
      {isActive ? (
        sortDir === "asc" ? (
          <ArrowUp className="h-3.5 w-3.5" />
        ) : (
          <ArrowDown className="h-3.5 w-3.5" />
        )
      ) : (
        <ArrowUpDown className="h-3.5 w-3.5 opacity-50" />
      )}
    </button>
  );
}

export function TransactionsView({ currency }: TransactionsViewProps) {
  const [search, setSearch] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [sortField, setSortField] = useState<TransactionSortField>("date");
  const [sortDir, setSortDir] = useState<SortDirection>("desc");

  const handleSort = (field: TransactionSortField) => {
    if (field === sortField) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir("desc");
    }
  };

  const filtered = useMemo(() => {
    let result = [...transactions];

    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (t) =>
          t.description.toLowerCase().includes(q) ||
          t.asset.toLowerCase().includes(q) ||
          t.reference.toLowerCase().includes(q)
      );
    }

    if (typeFilter !== "all") {
      result = result.filter((t) => t.type === typeFilter);
    }

    if (statusFilter !== "all") {
      result = result.filter((t) => t.status === statusFilter);
    }

    result.sort((a, b) => {
      let cmp = 0;
      switch (sortField) {
        case "date":
          cmp = a.date.getTime() - b.date.getTime();
          break;
        case "type":
          cmp = a.type.localeCompare(b.type);
          break;
        case "asset":
          cmp = a.asset.localeCompare(b.asset);
          break;
        case "amount":
          cmp = a.amount - b.amount;
          break;
        case "status":
          cmp = a.status.localeCompare(b.status);
          break;
      }
      return sortDir === "asc" ? cmp : -cmp;
    });

    return result;
  }, [search, typeFilter, statusFilter, sortField, sortDir]);

  const summary = useMemo(() => getTransactionSummary(), []);

  return (
    <div className="flex flex-col h-full">
      {/* Filter Bar */}
      <div className="shrink-0 px-6 py-4 border-b border-border bg-background">
        <div className="flex items-center gap-3 flex-wrap">
          <div className="relative flex-1 min-w-[200px] max-w-[300px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search transactions..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9 h-9"
            />
          </div>
          <Select value={typeFilter} onValueChange={setTypeFilter}>
            <SelectTrigger className="w-[170px] h-9">
              <Filter className="h-3.5 w-3.5 mr-1.5 text-muted-foreground" />
              <SelectValue placeholder="Type" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Types</SelectItem>
              {Object.entries(typeLabels).map(([val, label]) => (
                <SelectItem key={val} value={val}>{label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[150px] h-9">
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="completed">Completed</SelectItem>
              <SelectItem value="pending">Pending</SelectItem>
              <SelectItem value="invoiced">Invoiced</SelectItem>
              <SelectItem value="overdue">Overdue</SelectItem>
            </SelectContent>
          </Select>
          {(search || typeFilter !== "all" || statusFilter !== "all") && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setSearch("");
                setTypeFilter("all");
                setStatusFilter("all");
              }}
              className="text-xs"
            >
              Clear filters
            </Button>
          )}
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-auto">
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-muted z-10">
            <tr className="border-b border-border">
              <th className="text-left px-4 py-3">
                <SortableHeader label="Date" field="date" sortField={sortField} sortDir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-3">
                <SortableHeader label="Type" field="type" sortField={sortField} sortDir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-3">
                <SortableHeader label="Asset" field="asset" sortField={sortField} sortDir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-3 font-medium">Description</th>
              <th className="text-right px-4 py-3 font-medium">Qty</th>
              <th className="text-right px-4 py-3 font-medium">Unit Price</th>
              <th className="text-right px-4 py-3">
                <SortableHeader label="Amount" field="amount" sortField={sortField} sortDir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-3">
                <SortableHeader label="Status" field="status" sortField={sortField} sortDir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-3 font-medium">Reference</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((tx) => (
              <tr key={tx.id} className="border-b border-border hover:bg-muted/30 transition-colors">
                <td className="px-4 py-3 text-muted-foreground whitespace-nowrap">
                  {format(tx.date, "MMM dd, yyyy")}
                </td>
                <td className="px-4 py-3">
                  <Badge variant="outline" className="text-xs font-normal">
                    {typeLabels[tx.type]}
                  </Badge>
                </td>
                <td className="px-4 py-3 font-medium">{tx.asset}</td>
                <td className="px-4 py-3 text-muted-foreground max-w-[200px] truncate">{tx.description}</td>
                <td className="px-4 py-3 text-right font-mono text-xs">{tx.quantity.toLocaleString("en-US")}</td>
                <td className="px-4 py-3 text-right font-mono text-xs">
                  {formatCurrency(tx.unitPrice, currency)}
                </td>
                <td className={`px-4 py-3 text-right font-medium ${tx.amount >= 0 ? "text-status-operational" : "text-severity-critical"}`}>
                  {formatCurrency(tx.amount, currency)}
                </td>
                <td className="px-4 py-3">
                  <Badge variant="outline" className={`text-xs capitalize ${statusColors[tx.status]}`}>
                    {tx.status}
                  </Badge>
                </td>
                <td className="px-4 py-3 text-xs text-muted-foreground font-mono">{tx.reference}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Footer */}
      <div className="shrink-0 px-6 py-3 border-t border-border bg-background flex items-center justify-between text-xs text-muted-foreground">
        <span>{filtered.length} transactions</span>
        <div className="flex items-center gap-4">
          <span>
            Revenue: <span className="text-status-operational font-medium">{formatCurrency(summary.totalRevenue, currency)}</span>
          </span>
          <span>
            Expense: <span className="text-severity-critical font-medium">{formatCurrency(summary.totalExpense, currency)}</span>
          </span>
          <span>
            Net: <span className={`font-medium ${summary.net >= 0 ? "text-status-operational" : "text-severity-critical"}`}>
              {formatCurrency(summary.net, currency)}
            </span>
          </span>
        </div>
      </div>
    </div>
  );
}
