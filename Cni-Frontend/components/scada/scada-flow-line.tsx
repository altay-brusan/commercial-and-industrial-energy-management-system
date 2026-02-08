import type { FlowConnection } from "@/lib/scada/types";

interface ScadaFlowLineProps {
  connection: FlowConnection;
  pathD: string;
  id: string;
}

export function ScadaFlowLine({ connection, pathD, id }: ScadaFlowLineProps) {
  const isIdle = connection.direction === "idle";
  const isReverse = connection.direction === "reverse";

  // Duration inversely proportional to power (higher power = faster dots)
  const baseDur = Math.max(2, 6 - connection.power * 0.4);
  const dur = `${baseDur.toFixed(1)}s`;

  return (
    <g>
      {/* Background line — thin gray */}
      <path
        d={pathD}
        fill="none"
        stroke="#d1d5db"
        strokeWidth="1.5"
        strokeDasharray={isIdle ? "6 4" : undefined}
        opacity={isIdle ? 0.3 : 0.4}
      />

      {/* Animated gold dots */}
      {!isIdle &&
        [0, 0.25, 0.5, 0.75].map((offset, i) => (
          <circle
            key={`${id}-dot-${i}`}
            r="3"
            fill="#FBBF24"
            opacity="0.85"
          >
            <animateMotion
              dur={dur}
              repeatCount="indefinite"
              begin={`${(offset * baseDur).toFixed(2)}s`}
              keyPoints={isReverse ? "1;0" : "0;1"}
              keyTimes="0;1"
              calcMode="linear"
            >
              <mpath href={`#${id}`} />
            </animateMotion>
          </circle>
        ))}

      {/* Reference path for animateMotion */}
      <path id={id} d={pathD} fill="none" stroke="none" />

      {/* Power label centered on the path */}
      {!isIdle && connection.power > 0.01 && (
        <text
          fill="#9ca3af"
          fontSize="10"
          fontWeight="500"
          textAnchor="middle"
        >
          <textPath href={`#${id}`} startOffset="50%">
            {connection.power} MW
          </textPath>
        </text>
      )}
    </g>
  );
}
