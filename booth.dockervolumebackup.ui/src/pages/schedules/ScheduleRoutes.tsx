import Schedules from './Schedules.tsx';

const ScheduleRoutes = {
	path: 'schedules',
	children: [
		{
			index: true,
			element: <Schedules />,
		}
	]
};

export default ScheduleRoutes;
