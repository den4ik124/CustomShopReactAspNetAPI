import { Formik } from 'formik';
import React, { useState } from 'react'
import { Modal } from 'semantic-ui-react'
import { number } from 'yup';
import { Product } from '../../../models/product';
import { useStore } from '../../../stores/store';
import ProductModalForm from './ProductModalForm';

interface Props{
    trigger: React.ReactNode
    product: Product
}

function EditProductItem({trigger, product}  : Props) {
  const [open, setOpen] = useState(false)
  const {productStore} = useStore();
    
function handleProductEditing({ values, setErrors }: {
  values: {
    id: string;
    title: string;
    price: number;
    description: string;
    error: null;
  }; setErrors: (errors: import("formik").FormikErrors<{ title: string; price: number; description: string; error: string | null; }>) => void;
}): void {
      console.log(values);
      //TODO: change to update item 
      //productStore.createProduct(values);
      setOpen(false);

      // product.title = values.title;
      // product.description = values.description;
      // product.price = values.price;

}

  return (
    <Modal
      onClose={() =>setOpen(false)}
      onOpen={() => setOpen(true)}
      open={open}
      trigger={trigger}
    >
      <Modal.Header>Product editing</Modal.Header>
      <Modal.Content>
            <Formik 
                initialValues ={{
                    id: product.id, 
                    title: product.title,
                    price: product.price,
                    description: product.description,
                    error: null
            }}
            onSubmit={(initialValues, {setErrors}) => handleProductEditing({ values: initialValues, setErrors })}
            >
            {({handleSubmit, isSubmitting, errors}) => (
              <ProductModalForm 
                handleSubmit={handleSubmit} 
                isSubmitting={isSubmitting}
                applyButtonContent={'Confirm editing'}/>
            )}
        </Formik>

      </Modal.Content>
    </Modal>
  )

}

export default EditProductItem