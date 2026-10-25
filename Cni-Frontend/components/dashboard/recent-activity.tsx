import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";

interface Activity {
  id: string;
  time: string;
  event: string;
  source: "solar" | "grid" | "battery";
  value: string;
  status: "active" | "completed" | "scheduled";
}

const activities: Activity[] = [
  {
    id: "1",
    time: "14:32",
    event: "Peak demand detected",
    source: "grid",
    value: "8.7 kW",
    status: "completed",
  },
  {
    id: "2",
    time: "12:15",
    event: "Solar export started",
    source: "solar",
    value: "+2.1 kW",
    status: "active",
  },
  {
    id: "3",
    time: "10:00",
    event: "Battery charging",
    source: "battery",
    value: "85%",
    status: "active",
  },
  {
    id: "4",
    time: "08:30",
    event: "Morning peak consumption",
    source: "grid",
    value: "5.2 kW",
    status: "completed",
  },
  {
    id: "5",
    time: "06:00",
    event: "Off-peak charging scheduled",
    source: "battery",
    value: "100%",
    status: "scheduled",
  },
];

const sourceColors = {
  solar: "bg-energy-solar/10 text-energy-solar border-energy-solar/20",
  grid: "bg-energy-grid/10 text-energy-grid border-energy-grid/20",
  battery: "bg-energy-battery/10 text-energy-battery border-energy-battery/20",
};

const statusColors = {
  active: "bg-energy-solar/10 text-energy-solar",
  completed: "bg-secondary text-muted-foreground",
  scheduled: "bg-energy-battery/10 text-energy-battery",
};

export function RecentActivity() {
  return (
    <div className="px-6 py-6 border-t border-border">
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Recent Activity</h2>
        <p className="text-sm text-muted-foreground">
          Latest energy events and actions
        </p>
      </div>

      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead className="w-[80px]">Time</TableHead>
            <TableHead>Event</TableHead>
            <TableHead>Source</TableHead>
            <TableHead className="text-right">Value</TableHead>
            <TableHead className="text-right">Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {activities.map((activity) => (
            <TableRow key={activity.id} className="hover:bg-muted/50">
              <TableCell className="font-mono text-sm text-muted-foreground">
                {activity.time}
              </TableCell>
              <TableCell className="font-medium">{activity.event}</TableCell>
              <TableCell>
                <Badge
                  variant="outline"
                  className={`capitalize ${sourceColors[activity.source]}`}
                >
                  {activity.source}
                </Badge>
              </TableCell>
              <TableCell className="text-right font-mono">
                {activity.value}
              </TableCell>
              <TableCell className="text-right">
                <Badge
                  variant="secondary"
                  className={`capitalize ${statusColors[activity.status]}`}
                >
                  {activity.status}
                </Badge>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
