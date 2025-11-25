import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "./ui/dialog";
import { Button } from "./ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  AreaChart,
  Area,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import { Calendar, Clock, Sparkles, TrendingUp } from "lucide-react";
import { Separator } from "./ui/separator";

interface UsageDetailsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UsageDetailsDialog({ open, onOpenChange }: UsageDetailsDialogProps) {
  const [timeRange, setTimeRange] = useState("7d");
  const [selectedModel, setSelectedModel] = useState("all");

  // Mock data for daily usage (last 7 days)
  const dailyUsageData = [
    { date: "Nov 1", messages: 89, tokensIn: 15234, tokensOut: 10876, totalTokens: 26110 },
    { date: "Nov 2", messages: 124, tokensIn: 21456, tokensOut: 15234, totalTokens: 36690 },
    { date: "Nov 3", messages: 156, tokensIn: 28934, tokensOut: 19876, totalTokens: 48810 },
    { date: "Nov 4", messages: 203, tokensIn: 35678, tokensOut: 24567, totalTokens: 60245 },
    { date: "Nov 5", messages: 178, tokensIn: 31245, tokensOut: 21890, totalTokens: 53135 },
    { date: "Nov 6", messages: 145, tokensIn: 25678, tokensOut: 18234, totalTokens: 43912 },
    { date: "Today", messages: 92, tokensIn: 16234, tokensOut: 11456, totalTokens: 27690 },
  ];

  // Mock data for hourly usage (last 24 hours)
  const hourlyUsageData = [
    { hour: "00:00", messages: 5, tokens: 1234 },
    { hour: "01:00", messages: 3, tokens: 876 },
    { hour: "02:00", messages: 2, tokens: 567 },
    { hour: "03:00", messages: 1, tokens: 234 },
    { hour: "04:00", messages: 2, tokens: 456 },
    { hour: "05:00", messages: 4, tokens: 987 },
    { hour: "06:00", messages: 12, tokens: 2456 },
    { hour: "07:00", messages: 23, tokens: 4567 },
    { hour: "08:00", messages: 34, tokens: 6789 },
    { hour: "09:00", messages: 45, tokens: 8912 },
    { hour: "10:00", messages: 52, tokens: 10234 },
    { hour: "11:00", messages: 48, tokens: 9456 },
    { hour: "12:00", messages: 41, tokens: 8123 },
    { hour: "13:00", messages: 39, tokens: 7654 },
    { hour: "14:00", messages: 46, tokens: 9012 },
    { hour: "15:00", messages: 51, tokens: 10123 },
    { hour: "16:00", messages: 44, tokens: 8765 },
    { hour: "17:00", messages: 38, tokens: 7456 },
    { hour: "18:00", messages: 29, tokens: 5678 },
    { hour: "19:00", messages: 21, tokens: 4123 },
    { hour: "20:00", messages: 18, tokens: 3567 },
    { hour: "21:00", messages: 15, tokens: 2987 },
    { hour: "22:00", messages: 11, tokens: 2234 },
    { hour: "23:00", messages: 8, tokens: 1678 },
  ];

  // Mock data for usage by model with cost and efficiency
  const modelUsageData = [
    { 
      model: "GPT-4", 
      messages: 2847, 
      tokens: 771491, 
      percentage: 42,
      cost: 23.14,
      costPerMessage: 0.0081,
      costPer1kTokens: 0.030,
      efficiency: 271
    },
    { 
      model: "GPT-4 Turbo", 
      messages: 1523, 
      tokens: 486086, 
      percentage: 26,
      cost: 9.72,
      costPerMessage: 0.0064,
      costPer1kTokens: 0.020,
      efficiency: 319
    },
    { 
      model: "Claude 3 Opus", 
      messages: 892, 
      tokens: 300530, 
      percentage: 16,
      cost: 9.01,
      costPerMessage: 0.0101,
      costPer1kTokens: 0.030,
      efficiency: 337
    },
    { 
      model: "Claude 3 Sonnet", 
      messages: 634, 
      tokens: 166197, 
      percentage: 11,
      cost: 2.49,
      costPerMessage: 0.0039,
      costPer1kTokens: 0.015,
      efficiency: 262
    },
    { 
      model: "GPT-3.5 Turbo", 
      messages: 445, 
      tokens: 112357, 
      percentage: 6,
      cost: 0.56,
      costPerMessage: 0.0013,
      costPer1kTokens: 0.005,
      efficiency: 252
    },
    { 
      model: "Structured GPT", 
      messages: 156, 
      tokens: 63501, 
      percentage: 3,
      cost: 1.27,
      costPerMessage: 0.0081,
      costPer1kTokens: 0.020,
      efficiency: 407
    },
  ];

  // Mock data for weekly trend (last 4 weeks)
  const weeklyTrendData = [
    { week: "Week 1", messages: 543, tokensIn: 98765, tokensOut: 67234, totalTokens: 165999 },
    { week: "Week 2", messages: 678, tokensIn: 123456, tokensOut: 85678, totalTokens: 209134 },
    { week: "Week 3", messages: 812, tokensIn: 145678, tokensOut: 98765, totalTokens: 244443 },
    { week: "Week 4", messages: 987, tokensIn: 167890, tokensOut: 112345, totalTokens: 280235 },
  ];

  // Heatmap data: activity by hour (0-23) and day of week (Mon-Sun)
  const heatmapData = [
    // Monday
    [2, 1, 0, 0, 0, 3, 8, 15, 22, 28, 32, 29, 25, 24, 28, 31, 27, 21, 14, 9, 6, 4, 3, 2],
    // Tuesday
    [1, 0, 0, 0, 1, 4, 10, 18, 25, 31, 35, 33, 28, 26, 30, 33, 29, 23, 16, 10, 7, 5, 3, 1],
    // Wednesday
    [2, 1, 0, 0, 0, 3, 9, 16, 24, 30, 34, 32, 27, 25, 29, 32, 28, 22, 15, 9, 6, 4, 2, 1],
    // Thursday
    [1, 0, 0, 0, 2, 5, 11, 19, 27, 33, 37, 35, 30, 28, 32, 35, 31, 24, 17, 11, 8, 5, 3, 2],
    // Friday
    [2, 1, 0, 0, 1, 4, 10, 17, 23, 29, 31, 29, 24, 22, 25, 27, 23, 18, 12, 8, 5, 3, 2, 1],
    // Saturday
    [1, 0, 0, 0, 0, 1, 3, 6, 10, 14, 18, 20, 19, 17, 16, 14, 11, 8, 6, 4, 3, 2, 1, 0],
    // Sunday
    [0, 0, 0, 0, 0, 1, 2, 5, 8, 12, 15, 17, 16, 14, 13, 11, 9, 6, 4, 3, 2, 1, 0, 0]
  ];

  const daysOfWeek = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  const hoursOfDay = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '20', '21', '22', '23'];

  // Colors for charts
  const modelColors = {
    "GPT-4": "#8b5cf6",
    "GPT-4 Turbo": "#3b82f6",
    "Claude 3 Opus": "#f97316",
    "Claude 3 Sonnet": "#f59e0b",
    "GPT-3.5 Turbo": "#10b981",
    "Structured GPT": "#06b6d4",
  };

  const pieColors = ["#8b5cf6", "#3b82f6", "#f97316", "#f59e0b", "#10b981", "#06b6d4"];

  const formatNumber = (num: number) => {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + "M";
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1) + "K";
    }
    return num.toString();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-6xl max-h-[90vh] h-[90vh] p-0 gap-0 flex flex-col">
        <DialogHeader className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800">
          <DialogTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5" />
            Usage Analytics
          </DialogTitle>
          <DialogDescription>
            Detailed breakdown of your API usage and token consumption
          </DialogDescription>
        </DialogHeader>

        {/* Filters */}
        <div className="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex items-center gap-4">
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-gray-500" />
            <Select value={timeRange} onValueChange={setTimeRange}>
              <SelectTrigger className="w-[140px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="24h">Last 24 Hours</SelectItem>
                <SelectItem value="7d">Last 7 Days</SelectItem>
                <SelectItem value="30d">Last 30 Days</SelectItem>
                <SelectItem value="90d">Last 90 Days</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center gap-2">
            <Sparkles className="h-4 w-4 text-gray-500" />
            <Select value={selectedModel} onValueChange={setSelectedModel}>
              <SelectTrigger className="w-[180px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Models</SelectItem>
                <SelectItem value="gpt-4">GPT-4</SelectItem>
                <SelectItem value="gpt-4-turbo">GPT-4 Turbo</SelectItem>
                <SelectItem value="claude-opus">Claude 3 Opus</SelectItem>
                <SelectItem value="claude-sonnet">Claude 3 Sonnet</SelectItem>
                <SelectItem value="gpt-3.5">GPT-3.5 Turbo</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="ml-auto">
            <Button variant="outline" size="sm">
              Export Report
            </Button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-auto">
          <Tabs defaultValue="overview" className="p-6">
            <TabsList className="grid w-full grid-cols-3 sm:grid-cols-6 mb-6">
              <TabsTrigger value="overview">Overview</TabsTrigger>
              <TabsTrigger value="timeline">Timeline</TabsTrigger>
              <TabsTrigger value="models">By Model</TabsTrigger>
              <TabsTrigger value="cost">Cost</TabsTrigger>
              <TabsTrigger value="efficiency">Efficiency</TabsTrigger>
              <TabsTrigger value="distribution">Distribution</TabsTrigger>
            </TabsList>

            {/* Overview Tab */}
            <TabsContent value="overview" className="space-y-6">
              <div>
                <h3 className="font-semibold mb-4">Daily Message Volume</h3>
                <ResponsiveContainer width="100%" height={250}>
                  <AreaChart data={dailyUsageData}>
                    <defs>
                      <linearGradient id="colorMessages" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
                        <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" opacity={0.3} />
                    <XAxis dataKey="date" stroke="hsl(var(--muted-foreground))" />
                    <YAxis stroke="hsl(var(--muted-foreground))" />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "hsl(var(--popover))",
                        borderColor: "hsl(var(--border))",
                        borderWidth: "1px",
                        borderRadius: "6px",
                        color: "hsl(var(--popover-foreground))",
                      }}
                    />
                    <Area
                      type="monotone"
                      dataKey="messages"
                      stroke="#8b5cf6"
                      strokeWidth={2}
                      fillOpacity={1}
                      fill="url(#colorMessages)"
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Token Usage Trend</h3>
                <ResponsiveContainer width="100%" height={250}>
                  <LineChart data={dailyUsageData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" opacity={0.3} />
                    <XAxis dataKey="date" stroke="hsl(var(--muted-foreground))" />
                    <YAxis stroke="hsl(var(--muted-foreground))" tickFormatter={formatNumber} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "hsl(var(--popover))",
                        borderColor: "hsl(var(--border))",
                        borderWidth: "1px",
                        borderRadius: "6px",
                        color: "hsl(var(--popover-foreground))",
                      }}
                      formatter={(value: number) => formatNumber(value)}
                    />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="tokensIn"
                      stroke="#3b82f6"
                      strokeWidth={2}
                      name="Tokens In"
                    />
                    <Line
                      type="monotone"
                      dataKey="tokensOut"
                      stroke="#10b981"
                      strokeWidth={2}
                      name="Tokens Out"
                    />
                    <Line
                      type="monotone"
                      dataKey="totalTokens"
                      stroke="#8b5cf6"
                      strokeWidth={2}
                      name="Total"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div className="grid grid-cols-4 gap-4">
                <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                  <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Avg Messages/Day</p>
                  <p className="text-2xl font-semibold">142</p>
                  <p className="text-xs text-green-500 mt-1">↑ 12% from last week</p>
                </div>
                <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                  <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Avg Tokens/Day</p>
                  <p className="text-2xl font-semibold">38.5K</p>
                  <p className="text-xs text-green-500 mt-1">↑ 8% from last week</p>
                </div>
                <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                  <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Peak Hour</p>
                  <p className="text-2xl font-semibold">10 AM</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">52 messages</p>
                </div>
                <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                  <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Most Used Model</p>
                  <p className="text-2xl font-semibold">GPT-4</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">42% of total</p>
                </div>
              </div>
            </TabsContent>

            {/* Timeline Tab */}
            <TabsContent value="timeline" className="space-y-6">
              <div>
                <h3 className="font-semibold mb-4 flex items-center gap-2">
                  <Clock className="h-4 w-4" />
                  Hourly Activity (Last 24 Hours)
                </h3>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={hourlyUsageData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.1} />
                    <XAxis dataKey="hour" stroke="#6b7280" />
                    <YAxis stroke="#6b7280" />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#1f2937",
                        border: "1px solid #374151",
                        borderRadius: "6px",
                      }}
                    />
                    <Bar dataKey="messages" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Weekly Comparison</h3>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={weeklyTrendData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.1} />
                    <XAxis dataKey="week" stroke="#6b7280" />
                    <YAxis stroke="#6b7280" />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#1f2937",
                        border: "1px solid #374151",
                        borderRadius: "6px",
                      }}
                    />
                    <Legend />
                    <Bar dataKey="messages" fill="#8b5cf6" name="Messages" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Activity Heatmap Insights</h3>
                <div className="grid grid-cols-3 gap-4">
                  <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                    <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Busiest Day</p>
                    <p className="font-semibold">Thursday</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">Avg 203 messages</p>
                  </div>
                  <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                    <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Peak Hours</p>
                    <p className="font-semibold">9 AM - 3 PM</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">68% of activity</p>
                  </div>
                  <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                    <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Weekend Usage</p>
                    <p className="font-semibold">18%</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">Of total activity</p>
                  </div>
                </div>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Activity Heatmap - Hour of Day</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                  Visual representation of message activity by hour and day of week
                </p>
                <div className="overflow-x-auto">
                  <div className="inline-block min-w-full">
                    <div className="flex gap-1 mb-2 ml-12">
                      {hoursOfDay.map((hour, index) => (
                        <div 
                          key={hour} 
                          className="w-6 text-center text-xs text-gray-500 dark:text-gray-400"
                          style={{ display: index % 2 === 0 ? 'block' : 'none' }}
                        >
                          {hour}
                        </div>
                      ))}
                    </div>
                    {daysOfWeek.map((day, dayIndex) => (
                      <div key={day} className="flex items-center gap-1 mb-1">
                        <div className="w-10 text-xs text-gray-500 dark:text-gray-400 text-right">
                          {day}
                        </div>
                        {heatmapData[dayIndex].map((value, hourIndex) => {
                          const intensity = value / 37; // Max value is 37
                          const opacity = 0.1 + intensity * 0.9;
                          return (
                            <div
                              key={hourIndex}
                              className="w-6 h-6 rounded-sm hover:ring-2 hover:ring-purple-500 transition-all cursor-pointer"
                              style={{
                                backgroundColor: `rgba(139, 92, 246, ${opacity})`,
                              }}
                              title={`${day} ${hourIndex}:00 - ${value} messages`}
                            />
                          );
                        })}
                      </div>
                    ))}
                    <div className="flex items-center gap-2 mt-4 text-xs text-gray-500 dark:text-gray-400">
                      <span>Less</span>
                      <div className="flex gap-1">
                        {[0.1, 0.3, 0.5, 0.7, 0.9].map((opacity, i) => (
                          <div
                            key={i}
                            className="w-4 h-4 rounded-sm"
                            style={{ backgroundColor: `rgba(139, 92, 246, ${opacity})` }}
                          />
                        ))}
                      </div>
                      <span>More</span>
                    </div>
                  </div>
                </div>
              </div>
            </TabsContent>

            {/* Models Tab */}
            <TabsContent value="models" className="space-y-6">
              <div>
                <h3 className="font-semibold mb-4">Messages by Model</h3>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={modelUsageData} layout="vertical">
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.1} />
                    <XAxis type="number" stroke="#6b7280" />
                    <YAxis dataKey="model" type="category" stroke="#6b7280" width={120} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#1f2937",
                        border: "1px solid #374151",
                        borderRadius: "6px",
                      }}
                    />
                    <Bar dataKey="messages" radius={[0, 4, 4, 0]}>
                      {modelUsageData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={pieColors[index]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Token Consumption by Model</h3>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={modelUsageData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.1} />
                    <XAxis dataKey="model" stroke="#6b7280" />
                    <YAxis stroke="#6b7280" tickFormatter={formatNumber} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#1f2937",
                        border: "1px solid #374151",
                        borderRadius: "6px",
                      }}
                      formatter={(value: number) => formatNumber(value)}
                    />
                    <Bar dataKey="tokens" radius={[4, 4, 0, 0]}>
                      {modelUsageData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={pieColors[index]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div className="grid grid-cols-3 gap-4">
                {modelUsageData.slice(0, 3).map((model, index) => (
                  <div
                    key={model.model}
                    className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg"
                  >
                    <div className="flex items-center gap-2 mb-2">
                      <Sparkles className="h-4 w-4" style={{ color: pieColors[index] }} />
                      <p className="font-medium">{model.model}</p>
                    </div>
                    <p className="text-2xl font-semibold">{model.messages.toLocaleString()}</p>
                    <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                      {formatNumber(model.tokens)} tokens
                    </p>
                  </div>
                ))}
              </div>
            </TabsContent>

            {/* Cost Tab */}
            <TabsContent value="cost" className="space-y-6">
              <div>
                <h3 className="font-semibold mb-4">Cost Breakdown by Model</h3>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={modelUsageData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.1} />
                    <XAxis dataKey="model" stroke="#6b7280" />
                    <YAxis stroke="#6b7280" tickFormatter={(value) => `$${value}`} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#1f2937",
                        border: "1px solid #374151",
                        borderRadius: "6px",
                      }}
                      formatter={(value: number) => `$${value.toFixed(2)}`}
                    />
                    <Bar dataKey="cost" radius={[4, 4, 0, 0]}>
                      {modelUsageData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={pieColors[index]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Cost Metrics</h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                  <div className="p-4 border border-purple-200 dark:border-purple-800 bg-purple-50 dark:bg-purple-950/20 rounded-lg">
                    <p className="text-sm text-purple-600 dark:text-purple-400 mb-1">Total Cost</p>
                    <p className="text-2xl font-semibold text-purple-700 dark:text-purple-300">
                      ${modelUsageData.reduce((sum, m) => sum + m.cost, 0).toFixed(2)}
                    </p>
                    <p className="text-xs text-purple-600 dark:text-purple-400 mt-1">This billing period</p>
                  </div>
                  <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                    <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Avg Cost/Message</p>
                    <p className="text-2xl font-semibold">
                      ${(modelUsageData.reduce((sum, m) => sum + m.cost, 0) / modelUsageData.reduce((sum, m) => sum + m.messages, 0)).toFixed(4)}
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">Across all models</p>
                  </div>
                  <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg">
                    <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">Projected Monthly</p>
                    <p className="text-2xl font-semibold">
                      ${(modelUsageData.reduce((sum, m) => sum + m.cost, 0) * 4).toFixed(2)}
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">Based on current usage</p>
                  </div>
                </div>

                <div className="space-y-3">
                  <h4 className="font-medium">Cost Per Model</h4>
                  {modelUsageData.map((model, index) => (
                    <div key={model.model} className="p-3 border border-gray-200 dark:border-gray-800 rounded-lg">
                      <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center gap-2">
                          <div
                            className="w-3 h-3 rounded-full"
                            style={{ backgroundColor: pieColors[index] }}
                          />
                          <span className="font-medium">{model.model}</span>
                        </div>
                        <span className="font-semibold">${model.cost.toFixed(2)}</span>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm text-gray-600 dark:text-gray-400">
                        <div>
                          <span className="text-xs">Cost/Message:</span>
                          <span className="ml-1 font-mono">${model.costPerMessage.toFixed(4)}</span>
                        </div>
                        <div>
                          <span className="text-xs">Cost/1K Tokens:</span>
                          <span className="ml-1 font-mono">${model.costPer1kTokens.toFixed(3)}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </TabsContent>

            {/* Efficiency Tab */}
            <TabsContent value="efficiency" className="space-y-6">
              <div>
                <h3 className="font-semibold mb-4">Model Efficiency Comparison</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">
                  Efficiency score = Average tokens per message. Higher is better for complex tasks.
                </p>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={modelUsageData} layout="vertical">
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.1} />
                    <XAxis type="number" stroke="#6b7280" />
                    <YAxis dataKey="model" type="category" stroke="#6b7280" width={120} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#1f2937",
                        border: "1px solid #374151",
                        borderRadius: "6px",
                      }}
                      formatter={(value: number) => `${value} tokens/msg`}
                    />
                    <Bar dataKey="efficiency" radius={[0, 4, 4, 0]}>
                      {modelUsageData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={pieColors[index]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Efficiency Metrics</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {modelUsageData.map((model, index) => {
                    const costEfficiency = (model.tokens / model.cost).toFixed(0);
                    return (
                      <div
                        key={model.model}
                        className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg"
                      >
                        <div className="flex items-center gap-2 mb-3">
                          <Sparkles className="h-4 w-4" style={{ color: pieColors[index] }} />
                          <span className="font-medium">{model.model}</span>
                        </div>
                        <div className="space-y-2 text-sm">
                          <div className="flex justify-between">
                            <span className="text-gray-600 dark:text-gray-400">Tokens/Message:</span>
                            <span className="font-mono font-semibold">{model.efficiency}</span>
                          </div>
                          <div className="flex justify-between">
                            <span className="text-gray-600 dark:text-gray-400">Cost/Message:</span>
                            <span className="font-mono font-semibold">${model.costPerMessage.toFixed(4)}</span>
                          </div>
                          <div className="flex justify-between">
                            <span className="text-gray-600 dark:text-gray-400">Tokens/$1:</span>
                            <span className="font-mono font-semibold">{costEfficiency}</span>
                          </div>
                        </div>
                        <div className="mt-3 pt-3 border-t border-gray-200 dark:border-gray-800">
                          <div className="text-xs text-gray-500 dark:text-gray-400">
                            {model.efficiency > 300 ? '🚀 High verbosity' : 
                             model.efficiency > 250 ? '✅ Good balance' : 
                             '💬 Concise responses'}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Cost vs Performance Analysis</h3>
                <div className="p-4 border border-gray-200 dark:border-gray-800 rounded-lg bg-gray-50 dark:bg-gray-900">
                  <div className="space-y-3 text-sm">
                    <div className="flex items-start gap-2">
                      <span className="text-green-500">💡</span>
                      <div>
                        <p className="font-medium">Most Cost-Efficient:</p>
                        <p className="text-gray-600 dark:text-gray-400">
                          {modelUsageData.reduce((min, m) => m.costPerMessage < min.costPerMessage ? m : min).model} 
                          {' '}at ${modelUsageData.reduce((min, m) => m.costPerMessage < min.costPerMessage ? m : min).costPerMessage.toFixed(4)}/message
                        </p>
                      </div>
                    </div>
                    <div className="flex items-start gap-2">
                      <span className="text-blue-500">📊</span>
                      <div>
                        <p className="font-medium">Most Detailed Responses:</p>
                        <p className="text-gray-600 dark:text-gray-400">
                          {modelUsageData.reduce((max, m) => m.efficiency > max.efficiency ? m : max).model}
                          {' '}with {modelUsageData.reduce((max, m) => m.efficiency > max.efficiency ? m : max).efficiency} tokens/message avg
                        </p>
                      </div>
                    </div>
                    <div className="flex items-start gap-2">
                      <span className="text-purple-500">⚡</span>
                      <div>
                        <p className="font-medium">Best Value:</p>
                        <p className="text-gray-600 dark:text-gray-400">
                          GPT-4 Turbo balances cost ($0.0064/msg) with quality (319 tokens/msg)
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </TabsContent>

            {/* Distribution Tab */}
            <TabsContent value="distribution" className="space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <h3 className="font-semibold mb-4">Message Distribution</h3>
                  <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                      <Pie
                        data={modelUsageData}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ percentage }) => `${percentage}%`}
                        outerRadius={100}
                        fill="#8884d8"
                        dataKey="messages"
                      >
                        {modelUsageData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={pieColors[index]} />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "#1f2937",
                          border: "1px solid #374151",
                          borderRadius: "6px",
                        }}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                </div>

                <div>
                  <h3 className="font-semibold mb-4">Token Distribution</h3>
                  <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                      <Pie
                        data={modelUsageData}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ percentage }) => `${percentage}%`}
                        outerRadius={100}
                        fill="#8884d8"
                        dataKey="tokens"
                      >
                        {modelUsageData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={pieColors[index]} />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "#1f2937",
                          border: "1px solid #374151",
                          borderRadius: "6px",
                        }}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </div>

              <Separator />

              <div>
                <h3 className="font-semibold mb-4">Model Usage Breakdown</h3>
                <div className="space-y-3">
                  {modelUsageData.map((model, index) => (
                    <div key={model.model} className="space-y-2">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <div
                            className="w-3 h-3 rounded-full"
                            style={{ backgroundColor: pieColors[index] }}
                          />
                          <span className="font-medium">{model.model}</span>
                        </div>
                        <span className="text-sm text-gray-500 dark:text-gray-400">
                          {model.percentage}%
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 dark:bg-gray-800 rounded-full h-2">
                        <div
                          className="h-2 rounded-full transition-all"
                          style={{
                            width: `${model.percentage}%`,
                            backgroundColor: pieColors[index],
                          }}
                        />
                      </div>
                      <div className="flex justify-between text-sm text-gray-500 dark:text-gray-400">
                        <span>{model.messages.toLocaleString()} messages</span>
                        <span>{formatNumber(model.tokens)} tokens</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </TabsContent>
          </Tabs>
        </div>
      </DialogContent>
    </Dialog>
  );
}
