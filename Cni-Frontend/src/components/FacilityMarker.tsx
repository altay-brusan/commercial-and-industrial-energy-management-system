import React from 'react';
import ReactDOMServer from 'react-dom/server';
import { Marker } from '@react-google-maps/api';
import { Facility } from '../types/facility';
import MarkerIcon from './icons/MarkerIcon';

type FacilityMarkerProps = {
    facility: Facility;
    onClick: (facility: Facility) => void;
    activeColor?: string;
    inactiveColor?: string;
};

const FacilityMarker: React.FC<FacilityMarkerProps> = ({
    facility,
    onClick,
    activeColor = '#01b289',
    inactiveColor = '#F44336'
}) => {
    const handleMarkerClick = () => {
        onClick(facility);
    };

    const markerIconSvg = (
        <MarkerIcon
            color={facility.status === 'active' ? activeColor : activeColor}
        />
    );

    const getMarkerIcon = () => {
        const svgString = encodeURIComponent(
            ReactDOMServer.renderToString(markerIconSvg)
        );

        return {
            url: `data:image/svg+xml,${svgString}`,
        };
    };

    return (
        <Marker
            key={facility.id}
            position={{ lat: facility.latitude, lng: facility.longitude }}
            onClick={handleMarkerClick}
            title={facility.name}
            icon={getMarkerIcon()}
        />
    );
};

export default FacilityMarker;
