import { useState, useMemo } from 'react';
import { Table, Flex, Text, Checkbox } from '@radix-ui/themes';
import { ArrowUpIcon, ArrowDownIcon } from '@radix-ui/react-icons';
import { formatLongDateTime } from '@/utils/Formatting';

type DataType = string | number | boolean | Date;

type ColumnSort<T> = {
	column?: DataTableColumn<T>;
	ascending: boolean;
} 

export type DataTableColumn<T> = {
	id: number;
	heading: string;
	value: (row: T) => DataType | undefined;
	align?: 'left' | 'right' | 'center';
	sortable?: boolean;
	formatter?: (value?: DataType) => string;
	render?: (row: T) => React.ReactNode; 
}

export type DataTableProps<T, K> = {
	columns: DataTableColumn<T>[];
	data: T[];
	keyField: string;
	defaultSortColumn?: number; 
	defaultSortAscending?: boolean; 
	includeCheckBox?: boolean;
	selection?: Set<K>;
	onSelectionChange?: (selection: Set<K>) => void;
}

function sortData<T>(sortOrder: ColumnSort<T>, data: T[]): T[] {
	const sortedData = [...data];

	if (!sortOrder.column)
		return sortedData;

	sortedData.sort((a, b) => {
		if (!sortOrder.column)
			return 0;

		const value1 = sortOrder.column.value(a);
		const value2 = sortOrder.column.value(b);

		let order = 0;

		if ((typeof value1 == 'number') && (typeof value2 == 'number')) {
			order = value1 - value2;
		}
		else if ((typeof value1 == 'boolean') && (typeof value2 == 'boolean')) {
			order = (value1 ? 1 : 0) - (value2 ? 1 : 0);
		}
		else if ((value1 instanceof Date) && (value2 instanceof Date)) {
			order = (value1 ? value1.getTime() : 0) - (value2 ? value2.getTime() : 0);
		}
		else {
			if (value1 && value2 ) {
				order = value1.toString().localeCompare(value2.toString());
			}
			else if (!value1 && value2) {
				order = 1;
			}
			else if (value1 && !value2) {
				order = -1;
			}
			else if (!value1 && !value2) {
				order = 1;
			}
		}

		if (!sortOrder.ascending)
			order = -order;

		return order;
	});

	return sortedData;
}

function defaultFormater(value?: DataType): string {
	if (value == undefined)
		return '';
	else if (typeof value == 'number')
		return (value as number).toString();
	else if (typeof value == 'boolean')
		return (value as boolean).toString();
	else if (value instanceof Date)
		return formatLongDateTime(value as Date);
	else
		return value.toString();
};

export function DataTable<T,K>({
	columns,
	data,
	keyField,
	defaultSortColumn,
	defaultSortAscending = true,
	includeCheckBox = false,
	selection,
	onSelectionChange
	}: DataTableProps<T,K>) {

	const [sortOrder, setSortOrder] = useState<ColumnSort<T>>({ column: columns.find(x => x.id == defaultSortColumn), ascending: defaultSortAscending });
	const [selectAll, setSelectAll] = useState(false);
	const sortedData = useMemo(() => sortData(sortOrder, data), [sortOrder, data]) ;

	const sortOrderChanged = (selectedColumn: DataTableColumn<T>) => {
		setSortOrder({ column: selectedColumn, ascending: sortOrder.column && sortOrder.column.id == selectedColumn.id? !sortOrder.ascending : true });
	}

	const sortDisplay = (column: DataTableColumn<T>, ascending: boolean) : string => {
		return (sortOrder.column && (sortOrder.column.id == column.id) && (sortOrder.ascending == ascending)) ? 'inline' : 'none';
	}

	const getRowId = (row: T): K => {
		return row[keyField as keyof T] as K;
	}

	const toggleSelectAll = () => {
		const newValue = !selectAll;
		setSelectAll(newValue);

		const newSelection = newValue ? new Set(data.map(x => getRowId(x))) : new Set([]);

		onSelectionChange?.(newSelection);
	};

	const toggleSelected = (row: T) => {

		const rowId = getRowId(row);

		const newSelection = new Set(selection);
		if (newSelection.has(rowId)) {
			newSelection.delete(rowId)
		}
		else {
			newSelection.add(rowId);
		}

		onSelectionChange?.(newSelection);
	}

	return (
		<>
			<Table.Root variant="surface">
				<Table.Header>
					<Table.Row>
						{
							includeCheckBox ?
								(<Table.ColumnHeaderCell width="1px"><Checkbox checked={selectAll} onClick={() => toggleSelectAll()} /></Table.ColumnHeaderCell>)
								: null
						}
						{
							columns.map((column) => {
								return (
									<Table.ColumnHeaderCell key={'header_' + column.heading} align={column.align ? column.align : 'left'} onClick={() => column.sortable ? sortOrderChanged(column) : null}>
										<Flex direction="row" justify={column.align == 'right' ? 'end' : 'start'} gap="3">
											<Text size="3">{column.heading}</Text>
											{
												column.sortable ? 
													<>
														<ArrowUpIcon display={sortDisplay(column, true)} height="16" width="16" cursor="default" />
														<ArrowDownIcon display={sortDisplay(column, false)} height="16" width="16" cursor="default" />
													</>
													: null
											}
										</Flex>

									</Table.ColumnHeaderCell>
								)
							})
						}
					</Table.Row>
				</Table.Header>
				<Table.Body>
					{
						sortedData.map((row) => {

							return (
								<Table.Row key={getRowId(row)}>
									{
										includeCheckBox ?
											(<Table.Cell width="1px"><Checkbox checked={selection.has(getRowId(row))} onClick={() => toggleSelected(row)} /></Table.Cell>)
											: null
									}
									{
										columns.map((column) => {
											return (
												<Table.Cell key={'data_' + getRowId(row)  + '_' + column.heading} align={column.align? column.align: 'left'}>{column.render ? column.render(row) 
																		: column.formatter ? column.formatter(column.value(row)) : defaultFormater(column.value(row)) }</Table.Cell>
											)
										})
									}
								</Table.Row>
							)
						})
					}
				</Table.Body>
			</Table.Root>
		</>
	);
}