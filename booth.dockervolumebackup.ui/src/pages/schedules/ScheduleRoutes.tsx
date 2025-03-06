import ScheduleDetails from './ScheduleDetails.tsx';
import Schedules from './Schedules.tsx';

const ScheduleRoutes = {
	path: 'schedules',
	children: [
		{
			index: true,
			element: <Schedules />,
		},
		{
			path: ':scheduleId',
			element: <ScheduleDetails />,
		}
	]
};

export default ScheduleRoutes;
