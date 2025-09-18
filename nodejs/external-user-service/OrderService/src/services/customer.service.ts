/**
 * Customer service for Order Service
 * TypeScript equivalent to Python customer service
 */

import { Customer } from '../models/entities';
import { CreateCustomerDto, UpdateCustomerDto, CustomerDto } from '../models/dtos';
import { logger } from '../common/logger';

export class CustomerService {
    async getAllCustomers(): Promise<CustomerDto[]> {
        try {
            const customers = await Customer.findAll({
                order: [['created_at', 'DESC']]
            });
            return customers.map(customer => this.mapToDto(customer));
        } catch (error) {
            logger.error('Error getting all customers:', error);
            throw error;
        }
    }

    async getCustomerById(id: string): Promise<CustomerDto> {
        try {
            const customer = await Customer.findByPk(id);
            if (!customer) {
                const error = new Error(`Customer with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }
            return this.mapToDto(customer);
        } catch (error) {
            logger.error(`Error getting customer by ID ${id}:`, error);
            throw error;
        }
    }

    async createCustomer(customerData: CreateCustomerDto): Promise<CustomerDto> {
        try {
            const customer = await Customer.create({
                first_name: customerData.firstName,
                last_name: customerData.lastName,
                email: customerData.email,
                phone: customerData.phone,
                address: customerData.address
            });
            logger.info(`Customer created with ID: ${customer.id}`);
            return this.mapToDto(customer);
        } catch (error) {
            logger.error('Error creating customer:', error);
            throw error;
        }
    }

    async updateCustomer(id: string, customerData: UpdateCustomerDto): Promise<CustomerDto> {
        try {
            const customer = await Customer.findByPk(id);
            if (!customer) {
                const error = new Error(`Customer with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            await customer.update({
                first_name: customerData.firstName,
                last_name: customerData.lastName,
                email: customerData.email,
                phone: customerData.phone,
                address: customerData.address
            });

            logger.info(`Customer updated with ID: ${id}`);
            return this.mapToDto(customer);
        } catch (error) {
            logger.error(`Error updating customer with ID ${id}:`, error);
            throw error;
        }
    }

    async deleteCustomer(id: string): Promise<void> {
        try {
            const customer = await Customer.findByPk(id);
            if (!customer) {
                const error = new Error(`Customer with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            await customer.destroy();
            logger.info(`Customer deleted with ID: ${id}`);
        } catch (error) {
            logger.error(`Error deleting customer with ID ${id}:`, error);
            throw error;
        }
    }

    private mapToDto(customer: Customer): CustomerDto {
        return {
            id: customer.id,
            first_name: customer.first_name,
            last_name: customer.last_name,
            email: customer.email,
            phone: customer.phone,
            address: customer.address,
            created_at: customer.created_at,
            updated_at: customer.updated_at
        };
    }
}
